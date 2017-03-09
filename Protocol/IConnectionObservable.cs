using Protocol.Payloads;

namespace Protocol {
  public interface IConnectionObservable {
    event ConnectionEstablishedHandler OnEstablished;
    event ConnectionValidationFailedHandler OnValidationFailure;
  }

  #region event handlers
  public delegate void ConnectionValidationFailedHandler(string reason);
  public delegate void ConnectionEstablishedHandler(UserData userData);
  #endregion
}
