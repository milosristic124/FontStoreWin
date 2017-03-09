using Protocol.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TestUtilities.Protocol {
  public class MockedTransport : AConnectionTransport, ICallTracer {
    #region private data
    private CallTracer _tracer;
    private Action _disconnectCallback;
    private bool _connecting;
    private Dictionary<string, MockedBroadcastChannel> _channels;
    #endregion

    #region ctor
    public MockedTransport() : base() {
      _connecting = false;
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
      _connecting = true;

      TriggerConnectionAttempt();
    }

    public override void Disconnect(Action callback = null) {
      _tracer.RegisterCall("Disconnect");
      _channels.Clear();
      _disconnectCallback = callback;
    }

    public override IHttpRequest CreateHttpRequest(string endpoint) {
      RegisterCall("CreateHttpRequest");
      MockedHttpRequest request = new MockedHttpRequest(endpoint);
      request.OnRequestSent += Request_OnRequestSent;
      return request;
    }
    #endregion

    #region test methods
    public void SimulateDisconnection() {
      _disconnectCallback?.Invoke();
      _disconnectCallback = null;
    }

    public delegate void MessageSentHandler(MockedBroadcastResponse resp, string evt, dynamic payload);
    public event MessageSentHandler OnMessageSent;

    public delegate MockedHttpResponse HttpRequestSentHandler(MockedHttpRequest request, string body);
    public event HttpRequestSentHandler OnHttpRequestSent;

    public delegate bool ConnectionAttemptHandler();
    public event ConnectionAttemptHandler OnConnectionAttempt {
      add {
        _onConnectionAttempt += value;
        if (_connecting) {
          TriggerConnectionAttempt();
        }
      }
      remove {
        _onConnectionAttempt -= value;
      }
    }
    #endregion

    #region ICallTracer
    public void Verify(string methodName, int times) {
      _tracer.Verify(methodName, times);
    }

    public void RegisterCall(string name) {
      _tracer.RegisterCall(name);
    }
    #endregion

    #region private events
    private event ConnectionAttemptHandler _onConnectionAttempt;
    #endregion

    #region private methods
    private void Chan_OnMessageSent(MockedBroadcastChannel chan, MockedBroadcastResponse resp, string evt, dynamic payload) {
      OnMessageSent?.Invoke(resp, string.Format("{0}.{1}", chan.Topic, evt), payload);
    }

    private MockedHttpResponse Request_OnRequestSent(MockedHttpRequest request, string body) {
      return OnHttpRequestSent?.Invoke(request, body);
    }

    private void TriggerConnectionAttempt() {
      Task.Factory.StartNew(() => {
        bool? shouldConnect = _onConnectionAttempt?.Invoke();
        if (shouldConnect.HasValue) {
          _connecting = false;

          if (shouldConnect.Value) {
            Opened?.Invoke();
          } else {
            Error?.Invoke(new WebException("Connection fail simulation"));
          }
        }
      });
    }
    #endregion
  }
}
