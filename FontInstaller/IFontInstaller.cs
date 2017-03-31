using System.IO;
using System.Threading.Tasks;

namespace FontInstaller {
  public enum InstallationScope {
    Process,
    User,
    All
  }

  public interface IFontInstaller {
    Task<bool> InstallFont(string uid, InstallationScope scope, MemoryStream fontData);
    Task<bool> UnsintallFont(string uid, InstallationScope scope);
  }
}
