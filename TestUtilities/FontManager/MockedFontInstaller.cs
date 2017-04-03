using FontInstaller;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TestUtilities.FontManager {
  public class MockedFontInstaller : CallTracer, IFontInstaller {
    #region private data
    private Dictionary<string, InstallationScope> _installedFonts;
    #endregion

    #region ctor
    public MockedFontInstaller() {
      _installedFonts = new Dictionary<string, InstallationScope>();
    }
    #endregion

    #region test
    public delegate bool InstallationRequestHandler(string uid, InstallationScope scope);
    public event InstallationRequestHandler OnInstallRequest;

    public delegate bool UninstallationRequestHandler(string uid, InstallationScope scope);
    public event UninstallationRequestHandler OnUninstallRequest;

    public InstallationScope? FontInstallationScope(string uid) {
      if (_installedFonts.ContainsKey(uid)) {
        return _installedFonts[uid];
      }
      return null;
    }
    #endregion

    #region methods
    public InstallationScope GetFontInstallationScope(string uid) {
      InstallationScope scope;
      if (_installedFonts.TryGetValue(uid, out scope)) {
        return scope;
      }
      return InstallationScope.None;
    }

    public Task<FontAPIResult> InstallFont(string uid, InstallationScope scope, MemoryStream fontData) {
      RegisterCall("InstallFont");

      bool shouldInstall = OnInstallRequest?.Invoke(uid, scope) ?? true;
      if (!shouldInstall) {
        return Task.FromResult(FontAPIResult.Failure);
      }

      bool alreadyExists = _installedFonts.ContainsKey(uid);
      if (alreadyExists && _installedFonts[uid] == scope) {
        return Task.FromResult(FontAPIResult.Noop);
      }
      else if (alreadyExists && _installedFonts[uid] != scope) {
        _installedFonts[uid] = InstallationScope.All;
      } else {
        _installedFonts[uid] = scope;
      }
      return Task.FromResult(FontAPIResult.Success);
    }

    public Task<FontAPIResult> UninstallFont(string uid, InstallationScope scope) {
      RegisterCall("UnsintallFont");
      bool shouldUninstall = OnUninstallRequest?.Invoke(uid, scope) ?? true;
      if (!shouldUninstall) {
        return Task.FromResult(FontAPIResult.Failure);
      }

      if (_installedFonts.ContainsKey(uid)) {
        if (scope == InstallationScope.All || _installedFonts[uid] == scope) {
          _installedFonts.Remove(uid);
        }
        else if (scope == InstallationScope.User) {
          _installedFonts[uid] = InstallationScope.Process;
        }
        else {
          _installedFonts[uid] = InstallationScope.User;
        }
        return Task.FromResult(FontAPIResult.Success);
      }
      else {
        return Task.FromResult(FontAPIResult.Noop);
      }
    }
    #endregion
  }
}
