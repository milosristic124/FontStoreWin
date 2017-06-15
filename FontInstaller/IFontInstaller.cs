using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace FontInstaller {
  public enum FontAPIResult {
    Noop,
    Success,
    Failure
  }

  [FlagsAttribute]
  public enum InstallationScope {
    None = 0,
    Process = 1,
    User = 2
  }

  public interface IFontInstaller {
    #region methods
    InstallationScope GetFontInstallationScope(string uid);
    Task<FontAPIResult> InstallFont(string uid, InstallationScope scope, MemoryStream fontData);
    Task<FontAPIResult> UninstallFont(string uid, InstallationScope scope);
    Task UninstallAllFonts();
    #endregion
  }
}
