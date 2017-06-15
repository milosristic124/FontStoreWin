using FontInstaller;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Threading.Tasks;

namespace TestUtilities.FontManager {
  public class MockedFontInstaller : CallTracer, IFontInstaller {
    #region private data
    private List<string> _installedFonts;
    #endregion

    #region ctor
    public MockedFontInstaller() {
      _installedFonts = new List<string>();
    }
    #endregion

    #region test
    public delegate bool InstallationRequestHandler(string uid);
    public event InstallationRequestHandler OnInstallRequest;

    public delegate bool UninstallationRequestHandler(string uid);
    public event UninstallationRequestHandler OnUninstallRequest;

    public bool IsFontInstalled(string uid) {
      return _installedFonts.Contains(uid);
    }
    #endregion

    #region methods
    public Task<FontAPIResult> InstallFont(string uid, MemoryStream fontData) {
      RegisterCall("InstallFont");

      bool shouldInstall = OnInstallRequest?.Invoke(uid) ?? true;
      if (!shouldInstall) {
        return Task.FromResult(FontAPIResult.Failure);
      }

      bool alreadyExists = _installedFonts.Contains(uid);
      if (alreadyExists) {
        return Task.FromResult(FontAPIResult.Noop);
      }
      else {
        _installedFonts.Add(uid);
      }
      return Task.FromResult(FontAPIResult.Success);
    }

    public Task<FontAPIResult> UninstallFont(string uid) {
      RegisterCall("UninstallFont");
      bool shouldUninstall = OnUninstallRequest?.Invoke(uid) ?? true;
      if (!shouldUninstall) {
        return Task.FromResult(FontAPIResult.Failure);
      }

      if (_installedFonts.Contains(uid)) {
        _installedFonts.Remove(uid);
        return Task.FromResult(FontAPIResult.Success);
      } else {
        return Task.FromResult(FontAPIResult.Noop);
      }
    }

    public async Task UninstallAllFonts() {
      RegisterCall("UninstallAllFonts");
      await Task.Run(delegate {
        _installedFonts.Clear();
      });
    }
    #endregion
  }
}
