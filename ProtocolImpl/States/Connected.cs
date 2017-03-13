using Protocol.Transport;

namespace Protocol.Impl.States {
  class Connected : ConnectionState {
    #region private data
    private Payloads.UserData _userData;
    #endregion

    #region ctor
    public Connected(Connection connection, Payloads.UserData userData) : this(connection) {
      _userData = userData;
    }

    private Connected(Connection connection) : base("Connected", connection) {
    }
    #endregion

    #region methods
    public override bool CanTransitionTo<UpdatingCatalog>() {
      return WillTransition;
    }

    public override void Abort() {
    }

    public override void Stop() {
    }

    protected override void Start() {
      WillTransition = true;
      _context.TriggerConnectionEstablished(_userData);
    }
    #endregion
  }
}
