using FontInstaller;
using System;
using System.Threading.Tasks;
using Utilities.Extensions;

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
      _context.Storage.SaveFonts().ContinueWith(delegate {
        RegisterStorageEvents();
        _context.Storage.SynchronizeWithSystem(newFontCount => {
          WillTransition = true;
          FSM.State = new Running(newFontCount, _context);
        });
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
        Console.WriteLine("[{0}] [Installing] Font uninstalled in scope {1}: success = {2}", DateTime.Now.ToString("hh:mm:ss.fff"), scope, succeed);
      }
    }

    private void Storage_OnFontInstall(Storage.Data.Font font, InstallationScope scope, bool succeed) {
      if (scope.HasFlag(InstallationScope.User)) {
        _context.UserChannel.SendFontInstallationReport(font.UID, succeed);
      }
      else if (scope.HasFlag(InstallationScope.User)) {
        Console.WriteLine("[{0}] [Installing] Font installed in scope {1}: success = {2}", DateTime.Now.ToString("hh:mm:ss.fff"), scope, succeed);
      }
    }
    #endregion
  }
}
