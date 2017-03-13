using Protocol.Payloads;

namespace Protocol {
  public interface IConnectionObservable {
    event ConnectionEstablishedHandler OnEstablished;
    event ConnectionValidationFailedHandler OnValidationFailure;
    event CatalogUpdateFinished OnCatalogUpdateFinished;
  }

  #region event handlers
  public delegate void ConnectionValidationFailedHandler(string reason);
  public delegate void ConnectionEstablishedHandler(UserData userData);
  public delegate void CatalogUpdateFinished();
  #endregion
}
