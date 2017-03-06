using PhoenixSocket;
using System;

namespace Protocol.Transport.Phoenix {
  class BroadcastChannel : IBroadcastChannel {
    #region private data
    private Channel _channel;
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
    public IBroadcastResponse Join() {
      return new BroadcastResponse(_channel.Join());
    }

    public IBroadcastResponse Leave() {
      return new BroadcastResponse(_channel.Leave());
    }

    public IBroadcastResponse Send(string @event, dynamic payload) {
      return new BroadcastResponse(_channel.Push(@event, payload));
    }
    #endregion
  }
}
