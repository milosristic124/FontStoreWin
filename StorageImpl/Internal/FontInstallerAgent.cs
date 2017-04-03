using FontInstaller;
using Storage.Data;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Utilities.Threading;

namespace Storage.Impl.Internal {
  internal class FontInstallerAgent {
    #region private data
    private IFontInstaller _installer;
    private FSStorage _storage;
    private CancellationTokenSource _cancelSource;
    private ProcessingAgent _agent;
    #endregion

    #region delegates
    public delegate void ProcessingStartedHandler();
    public delegate void ProcessingFinishedHandler();
    #endregion

    #region events
    public event ProcessingStartedHandler OnInstallsStarted;
    public event ProcessingFinishedHandler OnInstallsFinished;
    #endregion

    #region ctor
    public FontInstallerAgent(FSStorage storage, IFontInstaller installer) {
      _storage = storage;
      _installer = installer;
      _cancelSource = new CancellationTokenSource();
      _agent = new ProcessingAgent(1, _cancelSource);

      _agent.OnProcessingFinished += _agent_OnProcessingFinished;
      _agent.OnProcessingStarted += _agent_OnProcessingStarted;
    }
    #endregion

    #region methods
    public void BeginInstalls() {
      _agent.Start();
    }

    public void PauseInstalls() {
      _agent.Stop();
    }

    public void AbortInstalls() {
      _agent.Stop();
      _cancelSource.Cancel();
    }

    public void QueueInstall(Font font, InstallationScope scope, Action<bool> then = null) {
      _agent.Enqueue(delegate {
        Stream cryptedData = _storage.ReadFontFile(font.UID).Result;
        MemoryStream decryptedData = DecryptFontData(cryptedData).Result;
        bool installed = _installer.InstallFont(font.UID, scope, decryptedData).Result;
        if (installed) {
          font.IsInstalled = true;
        }
        then?.Invoke(installed);
      });
    }

    public void QueueUninstall(Font font, InstallationScope scope, Action<bool> then = null) {
      _agent.Enqueue(delegate {
        bool uninstalled = _installer.UnsintallFont(font.UID, scope).Result;
        if (uninstalled) {
          font.IsInstalled = false;
        }
        then?.Invoke(uninstalled);
      });
    }
    #endregion

    #region private methods
    private Task<MemoryStream> DecryptFontData(Stream data) {
      return Task.Run(() => {
        MemoryStream res = new MemoryStream();
        data.CopyTo(res);
        return res;
      });
    }
    #endregion

    #region event handling
    private void _agent_OnProcessingStarted() {
      OnInstallsStarted?.Invoke();
    }

    private void _agent_OnProcessingFinished() {
      OnInstallsFinished?.Invoke();
    }
    #endregion
  }
}
