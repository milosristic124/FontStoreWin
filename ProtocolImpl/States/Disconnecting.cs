namespace Protocol.Impl.States {
  class Disconnecting : ConnectionState {
    #region private data
    private DisconnectReason _reason;
    private string _error;
    #endregion

    #region ctor
    public Disconnecting(Connection connection, DisconnectReason reason, string error = null): this("Disconnecting", connection) {
      _reason = reason;
      _error = error;
    }

    private Disconnecting(string name, Connection connection) : base(name, connection) {
    }
    #endregion

    #region methods
    public override void Abort() {
      Stop();
    }

    public override void Stop() {
    }

    protected override void Start() {
      string message = "Unknown reason.";
      switch (_reason) {
        case DisconnectReason.Quit:
          message = "Application quit.";
          break;

        case DisconnectReason.Logout:
          message = "User has logout.";
          break;

        case DisconnectReason.Error:
          message = string.Format("Application error: [{0}]", _error);
          break;
      }

      _context.Storage.DeactivateAllFonts(() => {
        WillTransition = true;
        _context.TriggerConnectionClosed();
      });
    }
    #endregion
  }
}
