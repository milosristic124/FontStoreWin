using Newtonsoft.Json;
using PhoenixSocket;
using System;

namespace Protocol.Transport.Phoenix {
  class BroadcastChannel : IBroadcastChannel {
    #region private data
    protected Channel _channel;
    #endregion

    #region properties
    public bool IsJoined {
      get { return _channel?.IsJoined() ?? false; }
    }
    #endregion

    #region ctor
    private BroadcastChannel() {
      throw new InvalidOperationException("A BroadcastChannel can only be instanciated via AConnectionTransport.Channel");
    }

    internal BroadcastChannel(Channel channel) {
      _channel = channel;
    }
    #endregion

    #region methods
    public IBroadcastChannelResult Join() {
      return new BroadcastChannelAction(_channel.Join());
    }

    public IBroadcastChannelResult Leave() {
      return new BroadcastChannelAction(_channel.Leave());
    }

    public IBroadcastResponse Send(string @event, dynamic payload) {
      return new BroadcastResponse(_channel.Push(@event, payload));
    }

    public IBroadcastChannel On(string evt, Action callback) {
      _channel.On(evt, () => {
        callback?.Invoke();
      });
      return this;
    }

    public IBroadcastChannel On<T>(string evt, Action<T> callback) where T: class {
      _channel.On(evt, (dynamic data) => {
        callback?.Invoke(JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(data)));
      });
      return this;
    }

    public IBroadcastChannel Off(string evt) {
      _channel.Off(evt);
      return this;
    }
    #endregion
  }
}
