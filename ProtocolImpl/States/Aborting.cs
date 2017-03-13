using Protocol.Transport;

namespace Protocol.Impl.States {
  class Aborting : ConnectionState {
    #region ctor
    public Aborting(Connection connection) : base("Aborting", connection) {
    }
    #endregion

    #region methods
    public override void Stop() {
    }

    public override void Abort() {
    }

    protected override void Start() {
      _context.Transport.Disconnect(() => {
        _context.Transport.AuthToken = null;
        FSM.State = new Idle(_context);
      });
    }
    #endregion
  }
}
