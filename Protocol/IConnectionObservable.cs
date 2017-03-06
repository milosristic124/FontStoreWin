using Protocol.Payloads;

namespace Protocol {
  public interface IConnectionObservable {
    event ConnectionEstablishedHandler OnEstablished;
    event ConnectionValidationFailedHandler OnValidationFailure;

    event FontDescriptionHandler OnFontDesctiptionReceived;
    event FontDeletedHandler OnFontDeleted;

    event FontActivationHandler OnFontActivated;
    event FontDeactivationHandler OnFontDeactivated;

    event UpdateFinishedHandler OnUpdateFinished;
  }

  #region event handlers
  public delegate void ConnectionValidationFailedHandler();
  public delegate void ConnectionEstablishedHandler(UserData userData);

  public delegate void FontDescriptionHandler(FontDescription fontDesc);
  public delegate void FontDeletedHandler(string uid);

  public delegate void FontActivationHandler(string uid);
  public delegate void FontDeactivationHandler(string uid);

  public delegate void UpdateFinishedHandler();
  #endregion
}
