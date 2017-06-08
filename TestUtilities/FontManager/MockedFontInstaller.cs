using FontInstaller;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Threading.Tasks;

namespace TestUtilities.FontManager {
  public class MockedFontInstaller : CallTracer, IFontInstaller {
    #region private data
    private Dictionary<string, InstallationScope> _installedFonts;
    #endregion

    #region properties
    public string UserFontDir { get; set; }
    public string PrivateFontDir { get; set; }
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
      else if (alreadyExists) {
        _installedFonts[uid] |= scope;
      }
      else {
        _installedFonts[uid] = scope;
      }
      return Task.FromResult(FontAPIResult.Success);
    }

    public Task<FontAPIResult> UninstallFont(string uid, InstallationScope scope) {
      RegisterCall("UninstallFont");
      bool shouldUninstall = OnUninstallRequest?.Invoke(uid, scope) ?? true;
      if (!shouldUninstall) {
        return Task.FromResult(FontAPIResult.Failure);
      }

      if (_installedFonts.ContainsKey(uid)) {
        if (_installedFonts[uid].HasFlag(scope)) {
          _installedFonts[uid] &= ~scope;
          if (_installedFonts[uid] == InstallationScope.None)
            _installedFonts.Remove(uid);
        }
        else {
          return Task.FromResult(FontAPIResult.Noop);
        }
        return Task.FromResult(FontAPIResult.Success);
      }
      else {
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
