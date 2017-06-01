using System;

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
        Console.WriteLine("[{0}] Updating fonts since {1}", DateTime.Now.ToString("hh:mm:ss.fff"), _context.Storage.LastFontStatusUpdate?.ToString("g"));
        _context.UserChannel.SendUpdateRequest(_context.Storage.LastFontStatusUpdate);
      });
    }
    #endregion

    #region event handling
    private void UserChannel_OnUpdateComplete() {
      WillTransition = true;
      FSM.State = new Installing(_context);
    }

    private void UserChannel_OnFontDeactivation(Payloads.TimestampedFontId fid) {
      _context.Storage.DeactivateFont(fid);
    }

    private void UserChannel_OnFontActivation(Payloads.TimestampedFontId fid) {
      _context.Storage.ActivateFont(fid);
    }
    #endregion
  }
}
