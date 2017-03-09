using System;
using Protocol.Transport;

namespace Protocol.Impl.States {
  class Aborting : ConnectionState {
    #region ctor
    public Aborting(Connection connection, IConnectionTransport transport) : base(connection, transport) {
    }
    #endregion

    #region methods
    public override void Stop() {
    }

    public override void Abort() {
    }

    protected override void Start() {
      _context.Disconnect(() => {
        _context.AuthToken = null;
        FSM.State = new Idle(_connection, _context);
      });
    }
    #endregion
  }
}
