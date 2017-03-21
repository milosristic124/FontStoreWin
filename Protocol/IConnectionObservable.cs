using Protocol.Payloads;

namespace Protocol {
  public interface IConnectionObservable {
    event ConnectionEstablishedHandler OnEstablished;
    event ConnectionValidationFailedHandler OnValidationFailure;
    event CatalogUpdateFinishedHandler OnCatalogUpdateFinished;
    event ConnectionClosedHandler OnDisconnected;
  }

  #region event handlers
  public delegate void ConnectionValidationFailedHandler(string reason);
  public delegate void ConnectionEstablishedHandler(UserData userData);
  public delegate void CatalogUpdateFinishedHandler();
  public delegate void ConnectionClosedHandler();
  #endregion
}
