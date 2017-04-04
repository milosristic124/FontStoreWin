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

    #region properties
    public int CommandCount {
      get {
        return _agent.CommandCount;
      }
    }
    #endregion

    #region delegates
    public delegate void ProcessingStartedHandler();
    public delegate void ProcessingFinishedHandler(int processedCommands);
    #endregion

    #region events
    public event ProcessingStartedHandler OnProcessingStarted;
    public event ProcessingFinishedHandler OnProcessingFinished;
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
    public void StartProcessing() {
      _agent.Start();
    }

    public void PauseProcessing() {
      _agent.Stop();
    }

    public void AbortProcessing() {
      _agent.Stop();
      _cancelSource.Cancel();
    }

    public void QueueInstall(Font font, InstallationScope scope, Action<FontAPIResult> then = null) {
      _agent.Enqueue(delegate {
        FontAPIResult result;

        using (Stream cryptedData = _storage.ReadFontFile(font.UID).Result) {
          using (MemoryStream decryptedData = DecryptFontData(cryptedData).Result) {
            result = _installer.InstallFont(font.UID, scope, decryptedData).Result;
          }
        }

        if (result != FontAPIResult.Failure) {
          font.IsInstalled = true;
        }
        then?.Invoke(result);
      });
    }

    public void QueueUninstall(Font font, InstallationScope scope, Action<FontAPIResult> then = null) {
      _agent.Enqueue(delegate {
        FontAPIResult result = _installer.UninstallFont(font.UID, scope).Result;
        if (result != FontAPIResult.Failure) {
          font.IsInstalled = false;
        }
        then?.Invoke(result);
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
      OnProcessingStarted?.Invoke();
    }

    private void _agent_OnProcessingFinished(int processedCommands) {
      OnProcessingFinished?.Invoke(processedCommands);
    }
    #endregion
  }
}
