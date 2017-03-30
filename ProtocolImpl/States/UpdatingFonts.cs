using Storage.Data;
using Utilities.Extensions;

namespace Protocol.Impl.States {
  class UpdatingFonts : ConnectionState {
    #region ctor
    public UpdatingFonts(Connection connection) : base("UpdatingFonts", connection) {
    }
    #endregion

    #region methods
    public override void Abort() {
      Stop();
    }

    public override void Stop() {
      _context.UserChannel.OnFontActivation -= UserChannel_OnFontActivation;
      _context.UserChannel.OnFontDeactivation -= UserChannel_OnFontDeactivation;
      _context.UserChannel.OnUpdateComplete -= UserChannel_OnUpdateComplete;
    }

    protected override void Start() {
      _context.UserChannel.OnFontActivation += UserChannel_OnFontActivation;
      _context.UserChannel.OnFontDeactivation += UserChannel_OnFontDeactivation;
      _context.UserChannel.OnUpdateComplete += UserChannel_OnUpdateComplete;

      _context.UserChannel.Join().Then(() => {
        _context.UserChannel.SendUpdateRequest();
      });
    }
    #endregion

    #region event handling
    private void UserChannel_OnUpdateComplete() {
      // local catalog is up-to-date, let's save it
      _context.Storage.Save().Then(delegate {
        WillTransition = true;
        FSM.State = new Installing(_context);
      });
    }

    private void UserChannel_OnFontDeactivation(string uid) {
      _context.Storage.DeactivateFont(uid);
    }

    private void UserChannel_OnFontActivation(string uid) {
      _context.Storage.ActivateFont(uid);
    }
    #endregion
  }
}
