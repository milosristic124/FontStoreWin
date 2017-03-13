using PhoenixSocket;
using System;

namespace Protocol.Transport.Phoenix {
  class BroadcastChannelAction : IBroadcastChannelResult {
    #region private data
    private Push _action;
    #endregion

    #region ctor
    private BroadcastChannelAction() {
      throw new InvalidOperationException("A BroadcastChannelAction can't be manually instanciated");
    }

    internal BroadcastChannelAction(Push action) {
      _action = action;
    }
    #endregion

    #region methods
    public IBroadcastChannelResult Recover(Action recoverBlock) {
      _action.Receive(ResponseStatus.Error.Name, delegate {
        recoverBlock?.Invoke();
      });
      return this;
    }

    public IBroadcastChannelResult Then(Action thenBlock) {
      _action.Receive(ResponseStatus.Ok.Name, delegate {
        thenBlock?.Invoke();
      });
      return this;
    }
    #endregion
  }
}
