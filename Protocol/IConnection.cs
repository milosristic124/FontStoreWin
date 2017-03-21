using Protocol.Payloads;
using Protocol.Transport;
using System;

namespace Protocol {
  public enum DisconnectReason {
    Quit,
    Logout,
    Error
  }

  public interface IConnection : IConnectionObservable {
    #region properties
    IConnectionTransport Transport { get; }
    UserData UserData { get; }

    TimeSpan AuthenticationRetryInterval { get; }
    TimeSpan ConnectionRetryInterval { get; }

    int DownloadParallelism { get; }
    TimeSpan DownloadTimeout { get; }
    #endregion

    #region methods
    void Connect(string email, string password);
    void Disconnect(DisconnectReason reason, string error = null);
    void UpdateCatalog();
    #endregion
  }
}
