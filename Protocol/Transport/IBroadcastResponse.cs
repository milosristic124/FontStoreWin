using System;

namespace Protocol.Transport {
  public interface IBroadcastResponse {
    IBroadcastResponse Receive(ResponseStatus status, Action<dynamic> callback);
  }
}
