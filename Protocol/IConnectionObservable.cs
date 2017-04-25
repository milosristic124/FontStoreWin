using Protocol.Payloads;

namespace Protocol {
  public interface IConnectionObservable {
    event ConnectionEstablishedHandler OnEstablished;
    event ConnectionValidationFailedHandler OnValidationFailure;
    event CatalogUpdateFinishedHandler OnCatalogUpdateFinished;
    event ConnectionClosedHandler OnConnectionClosed;
    event DisconnectionHandler OnDisconnected;
    event ConnectionTerminatedHandler OnConnectionTerminated;
  }

  #region event handlers
  public delegate void ConnectionValidationFailedHandler(string reason);
  public delegate void ConnectionEstablishedHandler(UserData userData);
  public delegate void CatalogUpdateFinishedHandler();
  public delegate void ConnectionClosedHandler();// connection closed after User disconnection
  public delegate bool DisconnectionHandler(); // connection disconnected after transport error
  public delegate void ConnectionTerminatedHandler(string reason); // connection closed after Server disconnection
  #endregion
}
