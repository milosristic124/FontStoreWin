using Protocol.Transport;
using System;
using System.Collections.Generic;

namespace TestUtilities.Protocol {
  class MockedBroadcastChannelResult : IBroadcastChannelResult {
    #region private data
    private ResponseStatus _status;
    private Dictionary<ResponseStatus, List<Action>> _callbacks;
    #endregion

    #region ctor
    public MockedBroadcastChannelResult() : this(null) {
    }

    public MockedBroadcastChannelResult(ResponseStatus status) {
      _status = status;
      _callbacks = new Dictionary<ResponseStatus, List<Action>>();
    }
    #endregion

    #region test methods
    public void SimulateResult(ResponseStatus status) {
      _status = status;

      List<Action> callbacks = null;
      if (_callbacks.TryGetValue(status, out callbacks)) {
        callbacks.ForEach(callback => {
          callback.Invoke();
        });
        _callbacks[status].Clear();
      }
    }
    #endregion

    #region methods
    public IBroadcastChannelResult Recover(Action recoverBlock) {
      AddCallback(ResponseStatus.Error, recoverBlock);
      return this;
    }

    public IBroadcastChannelResult Then(Action thenBlock) {
      AddCallback(ResponseStatus.Ok, thenBlock);
      return this;
    }
    #endregion

    #region private methods
    private void AddCallback(ResponseStatus status, Action callback) {
      if (!_callbacks.ContainsKey(status)) {
        _callbacks[status] = new List<Action>();
      }

      if (_status == status) {
        callback?.Invoke();
      } else {
        _callbacks[status].Add(callback);
      }
    }
    #endregion
  }
}
