using PhoenixSocket;
using System;

namespace Protocol.Transport.Phoenix {
  class BroadcastResponse: IBroadcastResponse {
    #region private data
    private Push _push;
    #endregion

    #region ctor
    private BroadcastResponse() {
      throw new InvalidOperationException("A BroadcastResponse can't be manually instanciated");
    }

    internal BroadcastResponse(Push push) {
      _push = push;
    }
    #endregion

    #region methods
    public IBroadcastResponse Receive(string status, Action<dynamic> callback) {
      return new BroadcastResponse(_push.Receive(status, callback));
    }
    #endregion
  }
}
