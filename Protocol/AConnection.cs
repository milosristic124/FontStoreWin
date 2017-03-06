using Protocol.Transport;
using System;

namespace Protocol {
  public abstract class AConnection: IConnection {
    #region properties
    public AConnectionTransport Transport { get; private set; }
    #endregion

    #region ctor
    public AConnection(AConnectionTransport transport) {
      Transport = transport;
    }
    #endregion

    #region methods
    public abstract void Connect(string email, string password);
    public abstract void Disconnect();
    public abstract void UpdateCatalog(DateTime? lastUpdate);
    public abstract void UpdateFontsStatus(DateTime? lastUpdate);
    #endregion

    #region IConnectionObservable
    public abstract event ConnectionEstablishedHandler OnEstablished;
    public abstract event ConnectionValidationFailedHandler OnValidationFailure;
    public abstract event FontDescriptionHandler OnFontDesctiptionReceived;
    public abstract event FontDeletedHandler OnFontDeleted;
    public abstract event FontActivationHandler OnFontActivated;
    public abstract event FontDeactivationHandler OnFontDeactivated;
    public abstract event UpdateFinishedHandler OnUpdateFinished;
    #endregion
  }
}
