using Protocol.Transport;
using System.Threading;
using System;
using Protocol.Payloads;

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
    public RetryAfterTimeInterval(Connection connection,
                                  IConnectionTransport transport,
                                  TimeSpan interval,
                                  Action retryCallback = null): this(connection, transport) {
      Callback = retryCallback;
      _interval = interval;
    }

    private RetryAfterTimeInterval(Connection connection, IConnectionTransport transport) : base(connection, transport) {
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
    public RetryAuthenticating(Connection connection, IConnectionTransport transport, string email, string password):
      base(connection, transport, connection.AuthenticationRetryInterval)
    {
      Callback = () => {
        FSM.State = new Authenticating(connection, transport, email, password);
      };
    }
  }

  class RetryConnecting : RetryAfterTimeInterval {
    public RetryConnecting(Connection connection, IConnectionTransport transport, UserData userData):
      base(connection, transport, connection.ConnectionRetryInterval)
    {
      Callback = () => {
        FSM.State = new Connecting(connection, transport, userData);
      };
    }
  }
}
