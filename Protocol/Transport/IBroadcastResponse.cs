using System;

namespace Protocol.Transport {
  public interface IBroadcastResponse {
    IBroadcastResponse Receive(string status, Action<dynamic> callback);
  }
}
