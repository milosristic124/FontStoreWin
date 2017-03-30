using Protocol;
using Protocol.Transport;
using Storage;

namespace Core {
  public static class Factory {
    public static IConnectionTransport InitializeTransport() {
      return new Protocol.Transport.Phoenix.ConnectionTransport();
    }

    public static IConnection InitializeServerConnection(IConnectionTransport transport, IFontStorage storage) {
      return new Protocol.Impl.Connection(transport, storage);
    }

    public static IFontStorage InitializeStorage(IHttpTransport transport) {
      return new Storage.Impl.FontStorage(transport, null);
    }
  }
}
