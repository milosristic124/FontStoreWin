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

    DateTime? LastCatalogUpdate { get; set; }
    DateTime? LastFontStatusUpdate { get; set; }

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
    void RemoveFont(string uid);
    void ActivateFont(string uid);
    void DeactivateFont(string uid);

    void SynchronizeWithSystem(Action then = null);
    void BeginSynchronization();
    void EndSynchronization();
    void AbortSynchronization();
    #endregion
  }
}
