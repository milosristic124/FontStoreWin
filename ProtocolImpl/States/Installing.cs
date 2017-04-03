using FontInstaller;

namespace Protocol.Impl.States {
  class Installing : ConnectionState {
    #region ctor
    public Installing(Connection connection) : this("Installing", connection) {
    }

    private Installing(string name, Connection connection) : base(name, connection) {
    }
    #endregion

    #region methods
    public override void Abort() {
      _context.Storage.AbortSynchronization();
    }

    public override void Stop() {
    }

    protected override void Start() {
      _context.Storage.OnFontInstall += Storage_OnFontInstall;
      _context.Storage.OnFontUninstall += Storage_OnFontUninstall;

      _context.Storage.SynchronizeWithSystem(delegate {
        WillTransition = true;
        FSM.State = new Running(_context);
      });
    }
    #endregion

    #region installation events handling
    private void Storage_OnFontUninstall(Storage.Data.Font font, InstallationScope scope, bool succeed) {
      if (scope == InstallationScope.User || scope == InstallationScope.All) {
        _context.UserChannel.SendFontUninstallationReport(font.UID, succeed);
      }
    }

    private void Storage_OnFontInstall(Storage.Data.Font font, InstallationScope scope, bool succeed) {
      if (scope == InstallationScope.User || scope == InstallationScope.All) {
         _context.UserChannel.SendFontInstallationReport(font.UID, succeed);
      }
    }
    #endregion
  }
}
