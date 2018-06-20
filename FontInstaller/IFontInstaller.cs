using System.IO;
using System.Threading.Tasks;

namespace FontInstaller {
  public enum FontAPIResult {
    Noop,
    Success,
    Failure
  }

  public interface IFontInstaller {
    #region methods
    Task<FontAPIResult> InstallFont(string uid, MemoryStream fontData);
    Task<FontAPIResult> UninstallFont(string uid);
    Task UninstallAllFonts();
    #endregion
  }
}
