using System;

namespace Protocol.Transport {
  public interface IBroadcastChannel {
    IBroadcastChannelResult Join();
    IBroadcastChannelResult Leave();

    IBroadcastResponse Send(string @event, dynamic payload);

    IBroadcastChannel On(string evt, Action callback);
    IBroadcastChannel On<T>(string evt, Action<T> callback) where T: class;
    IBroadcastChannel Off(string evt);
  }
}
