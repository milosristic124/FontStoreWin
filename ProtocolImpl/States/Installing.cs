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
      _context.Storage.SynchronizeWithSystem(delegate {
        WillTransition = true;
        FSM.State = new Running(_context);
      });
    }
    #endregion
  }
}
