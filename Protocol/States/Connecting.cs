using PhoenixSocket;

namespace Protocol.States {
  class Connecting : ConnectionState {
    #region private data
    private bool _connectionFinished;
    private string _email;
    private string _password;
    #endregion

    #region ctor
    public Connecting(ConnectionContext context, string email, string password) : this(context) {
      _email = email;
      _password = password;
    }

    private Connecting(ConnectionContext context) : base(context) {
      _context.Socket = new Socket(_context.EndPoint);

      _context.Socket.Opened += Socket_Opened;
      _context.Socket.Error += Socket_Error;
    }
    #endregion

    #region methods
    public override void Stop() {
      _context.Socket.Opened -= Socket_Opened;
      _context.Socket.Error -= Socket_Error;

      if (!_connectionFinished) {
        _context.Socket.Disconnect(() => { });
      }
    }

    protected override void Start() {
      _connectionFinished = false;
      _context.Socket.Connect();
    }

    private void Socket_Opened(object sender, System.EventArgs e) {
      _connectionFinished = true;
      FSM.State = new Validating(_context, _email, _password);
    }

    private void Socket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e) {
      _connectionFinished = true;
      throw new System.NotImplementedException(string.Format("[{0}] Connection failed", _email), e.Exception);
    }
    #endregion
  }
}
