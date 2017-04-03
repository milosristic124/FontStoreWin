using System.IO;
using System.Threading.Tasks;

namespace FontInstaller {
  public enum FontAPIResult {
    Noop,
    Success,
    Failure
  }

  public enum InstallationScope {
    None,
    Process,
    User,
    All
  }

  public interface IFontInstaller {
    InstallationScope GetFontInstallationScope(string uid);
    Task<FontAPIResult> InstallFont(string uid, InstallationScope scope, MemoryStream fontData);
    Task<FontAPIResult> UninstallFont(string uid, InstallationScope scope);
  }
}
