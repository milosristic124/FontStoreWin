using Protocol.Payloads;
using System;

namespace Protocol {
  public delegate void ConnectionValidationFailedHandler();
  public delegate void ConnectionEstablishedHandler(UserData userData);
  public delegate void ConnectionClosedHandler();
  public delegate void ConnectionTerminatedHandler();

  public interface IConnectionObservable {
    event ConnectionEstablishedHandler OnEstablished;
    event ConnectionValidationFailedHandler OnValidationFailure;
  }

  public interface IConnection: IConnectionObservable {
    void Connect(string email, string password);
    void Disconnect();
    void UpdateCatalog(DateTime? lastUpdate);
    void UpdateFontsStatus(DateTime? lastUpdate);
  }
}