namespace Core {
  public static class Factory {
    public static Protocol.IConnection InitializeServerConnection(Storage.IFontStorage storage) {
      Protocol.Transport.IConnectionTransport transport = new Protocol.Transport.Phoenix.ConnectionTransport();
      return new Protocol.Impl.Connection(transport, storage);
    }

    public static Storage.IFontStorage InitializeStorage() {
      return new Storage.Impl.FSFontStorage();
    }
  }
}
