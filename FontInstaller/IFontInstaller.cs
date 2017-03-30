using System.IO;
using System.Threading.Tasks;

namespace FontInstaller {
  public enum InstallationScope {
    Process,
    User,
    All
  }

  public interface IFontInstaller {
    Task InstallFont(string uid, InstallationScope scope, MemoryStream fontData);
    Task UnsintallFont(string uid, InstallationScope scope);
  }
}
