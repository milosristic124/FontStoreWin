using Protocol.Transport;
using System;
using System.Collections.Generic;

namespace TestUtilities.Protocol {
  public class MockedTransport : AConnectionTransport, ICallTracer {
    #region private data
    private CallTracer _tracer;
    private Action _disconnectCallback;

    private Dictionary<string, MockedBroadcastChannel> _channels;
    #endregion

    #region ctor
    public MockedTransport() : this("TestTransport") {
    }

    private MockedTransport(string endpoint) : base(endpoint) {
      _tracer = new CallTracer();
      _disconnectCallback = null;
      _channels = new Dictionary<string, MockedBroadcastChannel>();
    }
    #endregion

    #region events
    public override event TransportClosedHandler Closed;
    public override event TransportErrorHandler Error;
    public override event TransportOpenedHandler Opened;
    #endregion

    #region methods
    public override IBroadcastChannel Channel(string name) {
      _tracer.RegisterCall("Channel");

      if (!_channels.ContainsKey(name)) {
        MockedBroadcastChannel chan = new MockedBroadcastChannel(name);
        _channels[name] = chan;
        chan.OnMessageSent += Chan_OnMessageSent;
      }

      return _channels[name];
    }

    public override void Connect() {
      _tracer.RegisterCall("Connect");
    }

    public override void Disconnect(Action callback = null) {
      _tracer.RegisterCall("Disconnect");
      _channels.Clear();
      _disconnectCallback = callback;
    }
    #endregion

    #region test methods
    public void SimulateConnection() {
      Opened?.Invoke();
    }

    public void SimulateDisconnection() {
      _disconnectCallback?.Invoke();
      _disconnectCallback = null;
    }

    public delegate void MessageSentHandler(MockedBroadcastResponse resp, string evt, dynamic payload);
    public event MessageSentHandler OnMessageSent;
    #endregion

    #region ICallTracer
    public void Verify(string methodName, int times) {
      _tracer.Verify(methodName, times);
    }

    public void RegisterCall(string name) {
      _tracer.RegisterCall(name);
    }
    #endregion

    #region private methods
    private void Chan_OnMessageSent(MockedBroadcastChannel chan, MockedBroadcastResponse resp, string evt, dynamic payload) {
      OnMessageSent?.Invoke(resp, string.Format("{0}.{1}", chan.Topic, evt), payload);
    }
    #endregion
  }
}
