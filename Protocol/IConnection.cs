using Protocol.Payloads;
using Protocol.Transport;
using System;

namespace Protocol {
  public interface IConnection : IConnectionObservable {
    #region properties
    IConnectionTransport Transport { get; }
    UserData UserData { get; }

    TimeSpan AuthenticationRetryInterval { get; }
    TimeSpan ConnectionRetryInterval { get; }
    #endregion

    #region methods
    void Connect(string email, string password);
    void Disconnect();
    void UpdateCatalog();
    #endregion
  }
}
