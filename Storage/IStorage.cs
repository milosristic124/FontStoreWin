using FontInstaller;
using Protocol.Payloads;
using Storage.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage {
  public delegate void FontInstallationHandler(Font font, InstallationScope scope, bool succeed);
  public delegate void FontUninstallationHandler(Font font, InstallationScope scope, bool succeed);
  public delegate void FontActivationRequestHandler(Font font);
  public delegate void FontDeactivationRequestHandler(Font font);

  public interface IStorage {
    #region properties
    IFontInstaller Installer { get; }
    string SessionID { get; set; }

    int? LastCatalogUpdate { get; }
    int? LastFontStatusUpdate { get; }

    bool Loaded { get; }
    bool HasChanged { get; }

    IList<Family> ActivatedFamilies { get; }
    IList<Family> NewFamilies { get; }
    FamilyCollection FamilyCollection { get; }
    #endregion

    #region events
    event FontInstallationHandler OnFontInstall;
    event FontUninstallationHandler OnFontUninstall;
    event FontActivationRequestHandler OnFontActivationRequest;
    event FontDeactivationRequestHandler OnFontDeactivationRequest;
    #endregion

    #region methods
    Task SaveCredentials(string token);
    Task<string> LoadCredentials();
    Task CleanCredentials();

    Task LoadFonts();
    Task SaveFonts();
    void Clear();

    Font FindFont(string uid);

    Font AddFont(FontDescription description);
    void RemoveFont(TimestampedFontId fid);
    void ActivateFont(TimestampedFontId fid);
    void DeactivateFont(TimestampedFontId fid);

    void DeactivateAllFonts(Action then = null);

    void SynchronizeWithSystem(Action<int> then = null);
    void BeginSynchronization();
    void EndSynchronization();
    void AbortSynchronization();

    void ResetNewStatus();
    #endregion
  }
}
