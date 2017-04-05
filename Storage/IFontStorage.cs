using FontInstaller;
using Protocol.Payloads;
using Storage.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage {
  public delegate void FontInstallationHandler(Font font, InstallationScope scope, bool succeed);
  public delegate void FontUninstallationHandler(Font font, InstallationScope scope, bool succeed);

  public interface IFontStorage {
    #region properties
    IFontInstaller Installer { get; }
    string SessionID { get; set; }

    DateTime? LastCatalogUpdate { get; }
    DateTime? LastFontStatusUpdate { get; }

    bool Loaded { get; }
    bool HasChanged { get; }

    IList<Family> ActivatedFamilies { get; }
    IList<Family> NewFamilies { get; }
    FamilyCollection FamilyCollection { get; }
    #endregion

    #region events
    event FontInstallationHandler OnFontInstall;
    event FontUninstallationHandler OnFontUninstall;
    #endregion

    #region methods
    Task Load();
    Task Save();

    Font FindFont(string uid);

    Font AddFont(FontDescription description);
    void RemoveFont(FontId fid);
    void ActivateFont(FontId fid);
    void DeactivateFont(FontId fid);

    void DeactivateAllFonts(Action then = null);

    void SynchronizeWithSystem(Action then = null);
    void BeginSynchronization();
    void EndSynchronization();
    void AbortSynchronization();
    #endregion
  }
}
