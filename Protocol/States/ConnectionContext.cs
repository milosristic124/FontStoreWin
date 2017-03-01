using PhoenixSocket;
using Protocol.Payloads;

namespace Protocol.States {
  class ConnectionContext: IConnectionObservable {
    #region data
    public string EndPoint;
    public Socket Socket;
    public string AuthToken = null;
    #endregion

    #region methods
    public void TriggerConnectionEstablished(UserData userData) {
      AuthToken = userData.ReuseToken;
      OnEstablished?.Invoke(userData);
    }
    #endregion

    #region events
    public event ConnectionEstablishedHandler OnEstablished;
    public event ConnectionValidationFailedHandler OnValidationFailure;
    public event FontDescriptionHandler OnFontDesctiptionReceived;
    public event FontDeletedHandler OnFontDeleted;
    public event UpdateFinishedHandler OnUpdateFinished;
    public event FontActivationHandler OnFontActivated;
    public event FontDeactivationHandler OnFontDeactivated;
    #endregion
  }
}
