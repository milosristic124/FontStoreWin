using System;

namespace Protocol {
  public interface IConnection : IConnectionObservable {
    void Connect(string email, string password);
    void Disconnect();
    void UpdateCatalog(DateTime? lastUpdate);
    void UpdateFontsStatus(DateTime? lastUpdate);
  }
}
