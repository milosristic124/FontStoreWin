using Protocol.Impl;
using Protocol.Payloads;
using System;

namespace Protocol {
  public delegate void ConnectionValidationFailedHandler();
  public delegate void ConnectionEstablishedHandler(UserData userData);

  public delegate void FontDescriptionHandler(FontDescription fontDesc);
  public delegate void FontDeletedHandler(string uid);

  public delegate void FontActivationHandler(string uid);
  public delegate void FontDeactivationHandler(string uid);

  public delegate void UpdateFinishedHandler();

  public interface IConnectionObservable {
    event ConnectionEstablishedHandler OnEstablished;
    event ConnectionValidationFailedHandler OnValidationFailure;

    event FontDescriptionHandler OnFontDesctiptionReceived;
    event FontDeletedHandler OnFontDeleted;

    event FontActivationHandler OnFontActivated;
    event FontDeactivationHandler OnFontDeactivated;

    event UpdateFinishedHandler OnUpdateFinished;
  }

  public interface IConnection: IConnectionObservable {
    void Connect(string email, string password);
    void Disconnect();
    void UpdateCatalog(DateTime? lastUpdate);
    void UpdateFontsStatus(DateTime? lastUpdate);
  }
}