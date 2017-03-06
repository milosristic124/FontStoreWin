using System;

namespace Protocol.Transport {
  public interface IBroadcastChannel {
    IBroadcastResponse Join();
    IBroadcastResponse Leave();

    IBroadcastResponse Send(string @event, dynamic payload);
  }
}
