using Protocol.Transport;
using Protocol.Transport.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace TestUtilities.Protocol {
  public class MockedTransport : AConnectionTransport, ICallTracer {
    #region private data
    private CallTracer _tracer;
    private Action _disconnectCallback;
    private bool _connecting;
    private bool _disconnecting;
    private Dictionary<string, MockedBroadcastChannel> _channels;
    #endregion

    #region ctor
    public MockedTransport() : base() {
      _connecting = false;
      _disconnecting = false;
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
      _disconnecting = true;

      TriggerDisconnectionAttempt();
    }
    #endregion

    #region test methods
    public void SimulateMessage(string topic, string evt, dynamic payload = null) {
      MockedBroadcastChannel chan;
      if (_channels.TryGetValue(topic, out chan)) {
        chan.SimulateMessage(evt, payload);
      }
    }

    public void SimulateTermination() {
      Closed?.Invoke();
    }

    public delegate void MessageSentHandler(string evt, dynamic payload);
    public event MessageSentHandler OnMessageSent;

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

    public delegate bool DisconnectionAttemptHandler();
    public event DisconnectionAttemptHandler OnDisconnectAttempt {
      add {
        _onDisconnectionAttempt += value;
        if (_disconnecting) {
          TriggerDisconnectionAttempt();
        }
      }
      remove {
        _onDisconnectionAttempt -= value;
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
    private event DisconnectionAttemptHandler _onDisconnectionAttempt;
    #endregion

    #region private methods
    private void Chan_OnMessageSent(MockedBroadcastChannel chan, string evt, dynamic payload) {
      OnMessageSent?.Invoke(string.Format("{0}.{1}", chan.Topic, evt), payload);
    }

    private async void TriggerConnectionAttempt() {
      await Task.Run(() => {
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

    private async void TriggerDisconnectionAttempt() {
      await Task.Run(() => {
        bool? shouldDisconnect = _onDisconnectionAttempt?.Invoke();
        if (!shouldDisconnect.HasValue || shouldDisconnect.Value) {
          _disconnecting = false;
          _disconnectCallback?.Invoke();
        }
      });
    }
    #endregion
  }
}
