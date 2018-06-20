using System;

namespace Protocol.Transport {
  public interface IBroadcastChannel {
    bool IsJoined { get; }

    IBroadcastChannelResult Join();
    IBroadcastChannelResult Leave();

    void Send(string @event, dynamic payload);

    IBroadcastChannel On(string evt, Action callback);
    IBroadcastChannel On<T>(string evt, Action<T> callback) where T: class;
    IBroadcastChannel Off(string evt);
  }
}
