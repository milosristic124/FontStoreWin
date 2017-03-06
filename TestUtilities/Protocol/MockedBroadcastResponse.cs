using Protocol.Transport;
using System;
using System.Collections.Generic;

namespace TestUtilities.Protocol {
  public class MockedBroadcastResponse : IBroadcastResponse {
    #region private data
    private string _status;
    private dynamic _payload;
    private Dictionary<string, List<Action<dynamic>>> _callbacks;
    #endregion

    #region ctor
    public MockedBroadcastResponse() : this(null, null) {
    }

    public MockedBroadcastResponse(string status, dynamic payload = null) {
      _status = status;
      _payload = payload;
      _callbacks = new Dictionary<string, List<Action<dynamic>>>();
    }
    #endregion

    #region test methods
    public void SimulateReply(string status, dynamic payload) {
      _status = status;
      _payload = payload;

      List<Action<dynamic>> callbacks = null;
      if (_callbacks.TryGetValue(status, out callbacks)) {
        callbacks.ForEach(callback => {
          callback.Invoke(payload);
        });
        _callbacks[status].Clear();
      }
    }
    #endregion

    #region methods
    public IBroadcastResponse Receive(string status, Action<dynamic> callback) {
      if (!_callbacks.ContainsKey(status)) {
        _callbacks[status] = new List<Action<dynamic>>();
      }

      if (_status == status) {
        callback?.Invoke(_payload);
      }
      else {
        _callbacks[status].Add(callback);
      }

      return this;
    }
    #endregion
  }
}
