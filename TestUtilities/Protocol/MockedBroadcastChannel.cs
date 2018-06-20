using Protocol.Transport;
using System;
using System.Collections.Generic;

namespace TestUtilities.Protocol {
  internal interface ICallback {
    void Invoke(dynamic param);
  }

  internal class GenCallback<T>: ICallback where T: class {
    private Action<T> _action;

    public GenCallback(Action<T> action) {
      _action = action;
    }

    public void Invoke(dynamic param) {
      if (param != null)
        _action?.Invoke(param as T);
      else
        _action?.Invoke(null);
    }
  }

  internal class Callback: ICallback {
    private Action _action;

    public Callback(Action action) {
      _action = action;
    }

    public void Invoke(dynamic param) {
      _action?.Invoke();
    }
  }

  public class MockedBroadcastChannel : IBroadcastChannel {
    #region private data
    private Dictionary<string, List<ICallback>> _callbacks;
    #endregion

    #region properties
    public bool IsJoined { get; private set; }
    public string Topic { get; private set; }
    #endregion

    #region ctor
    public MockedBroadcastChannel(string topic) {
      _callbacks = new Dictionary<string, List<ICallback>>();
      Topic = topic;
    }
    #endregion

    #region test methods
    public delegate void ChannelSentHandler(MockedBroadcastChannel chan, string evt, dynamic payload);
    public event ChannelSentHandler OnMessageSent;

    public void SimulateMessage(string evt, dynamic payload = null) {
      List<ICallback> callbacks;
      if (_callbacks.TryGetValue(evt, out callbacks)) {
        callbacks.ForEach((callback) => {
          callback.Invoke(payload);
        });
      }
    }
    #endregion

    #region methods
    public IBroadcastChannelResult Join() {
      IsJoined = true;
      return new MockedBroadcastChannelResult(ResponseStatus.Ok);
    }

    public IBroadcastChannelResult Leave() {
      IsJoined = false;
      return new MockedBroadcastChannelResult(ResponseStatus.Ok);
    }

    public void Send(string @event, dynamic payload) {
      OnMessageSent?.Invoke(this, @event, payload);
    }

    public IBroadcastChannel On(string evt, Action callback) {
      if (!_callbacks.ContainsKey(evt)) {
        _callbacks[evt] = new List<ICallback>();
      }

      _callbacks[evt].Add(new Callback(callback));
      return this;
    }

    public IBroadcastChannel On<T>(string evt, Action<T> callback) where T : class {
      if (!_callbacks.ContainsKey(evt)) {
        _callbacks[evt] = new List<ICallback>();
      }

      _callbacks[evt].Add(new GenCallback<T>(callback));
      return this;
    }

    public IBroadcastChannel Off(string evt) {
      _callbacks.Remove(evt);
      return this;
    }
    #endregion
  }
}
