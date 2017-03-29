using System;

namespace Protocol.Transport {
  public interface IBroadcastChannelResult {
    IBroadcastChannelResult Then(Action thenBlock);
    IBroadcastChannelResult Recover(Action recoverBlock);
  }
}
