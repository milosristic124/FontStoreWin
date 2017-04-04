using Protocol.Payloads;

namespace Protocol {
  public interface IConnectionObservable {
    event ConnectionEstablishedHandler OnEstablished;
    event ConnectionValidationFailedHandler OnValidationFailure;
    event CatalogUpdateFinishedHandler OnCatalogUpdateFinished;
    event ConnectionClosedHandler OnConnectionClosed;
    event DisconnectionHandler OnDisconnected;
  }

  #region event handlers
  public delegate void ConnectionValidationFailedHandler(string reason);
  public delegate void ConnectionEstablishedHandler(UserData userData);
  public delegate void CatalogUpdateFinishedHandler();
  public delegate void ConnectionClosedHandler();
  public delegate void DisconnectionHandler(string reason);
  #endregion
}
