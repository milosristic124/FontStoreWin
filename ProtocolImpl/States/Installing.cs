using FontInstaller;
using System;

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
      Stop();
    }

    public override void Stop() {
      UnregisterStorageEvents();
    }

    protected override void Start() {
      RegisterStorageEvents();

      _context.Storage.SynchronizeWithSystem(delegate {
        WillTransition = true;
        FSM.State = new Running(_context);
      });
    }
    #endregion

    #region private methods
    private void RegisterStorageEvents() {
      _context.Storage.OnFontInstall += Storage_OnFontInstall;
      _context.Storage.OnFontUninstall += Storage_OnFontUninstall;
    }

    private void UnregisterStorageEvents() {
      _context.Storage.OnFontInstall -= Storage_OnFontInstall;
      _context.Storage.OnFontUninstall -= Storage_OnFontUninstall;
    }
    #endregion

    #region installation events handling
    private void Storage_OnFontUninstall(Storage.Data.Font font, InstallationScope scope, bool succeed) {
      if (scope.HasFlag(InstallationScope.User)) {
        _context.UserChannel.SendFontUninstallationReport(font.UID, succeed);
      } else if (scope.HasFlag(InstallationScope.User)) {
        Console.WriteLine("[Installing] Font uninstalled in scope {0}: success = {1}", scope, succeed);
      }
    }

    private void Storage_OnFontInstall(Storage.Data.Font font, InstallationScope scope, bool succeed) {
      if (scope.HasFlag(InstallationScope.User)) {
        _context.UserChannel.SendFontInstallationReport(font.UID, succeed);
      }
      else if (scope.HasFlag(InstallationScope.User)) {
        Console.WriteLine("[Installing] Font installed in scope {0}: success = {1}", scope, succeed);
      }
    }
    #endregion
  }
}
