using Protocol.Transport;
using System.Threading;
using System;

namespace Protocol.Impl.States {
  class RetryAfterTimeInterval : ConnectionState {
    #region private data
    private Timer _retryTimer;
    private TimeSpan _interval;
    #endregion

    #region properties
    public Action Callback { get; set; }
    #endregion

    #region ctor
    public RetryAfterTimeInterval(string name, 
                                  Connection connection,
                                  TimeSpan interval,
                                  Action retryCallback = null): this(name, connection) {
      Callback = retryCallback;
      _interval = interval;
    }

    private RetryAfterTimeInterval(string name, Connection connection) : base(name, connection) {
      _retryTimer = null;
      Callback = null;
      _interval = Timeout.InfiniteTimeSpan;
    }
    #endregion

    #region methods
    public override void Abort() {
      _retryTimer.Change(Timeout.Infinite, Timeout.Infinite);
      _retryTimer.Dispose();
      _retryTimer = null;
    }

    public override void Stop() {
      _retryTimer.Dispose();
      _retryTimer = null;
    }

    protected override void Start() {
      _retryTimer = new Timer(delegate {
        WillTransition = true; // just in case the callback change the FSM state
        Callback?.Invoke();
        WillTransition = false;
      }, null, _interval, Timeout.InfiniteTimeSpan);
    }
    #endregion
  }

  class RetryAuthenticating : RetryAfterTimeInterval {
    public RetryAuthenticating(Connection connection, Payloads.Authentication payload) :
      base("RetryAuthenticating", connection, connection.AuthenticationRetryInterval) {

      Init(connection, payload as Payloads.Connect);
      Init(connection, payload as Payloads.AutoConnect);
    }

    #region private methods
    private void Init(Connection connection, Payloads.AutoConnect payload) {
      if (payload != null) {
        Callback = () => {
          FSM.State = new Authenticating(connection, payload.AuthToken);
        };
      }
    }

    private void Init(Connection connection, Payloads.Connect payload) {
      if (payload != null) {
        Callback = () => {
          FSM.State = new Authenticating(connection, payload.Login, payload.Password);
        };
      }
    }
    #endregion
  }

  class RetryConnecting : RetryAfterTimeInterval {
    public RetryConnecting(Connection connection, Payloads.UserData userData):
      base("RetryConnecting", connection, connection.ConnectionRetryInterval)
    {
      Callback = () => {
        FSM.State = new Connecting(connection, userData);
      };
    }

    protected override void Start() {
      base.Start();
      _context.Transport.Disconnect();
    }
  }
}
