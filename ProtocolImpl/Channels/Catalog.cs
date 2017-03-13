using Protocol.Transport;

namespace Protocol.Impl.Channels {
  class Catalog {
    #region private data
    private IConnection _connection;
    private IBroadcastChannel _underlying;
    #endregion

    #region ctor
    public Catalog(IConnection connection) {
      _connection = connection;
      _underlying = connection.Transport.Channel("catalog");
    }
    #endregion

    #region methods
    public IBroadcastChannelResult Join() {
      _underlying.On("font:description", (Payloads.FontDescription desc) => {
        OnFontDescription?.Invoke(desc);
      });
      _underlying.On("font:deletion", (Payloads.FontId del) => {
        OnFontDeletion?.Invoke(del.UID);
      });
      _underlying.On("update:complete", () => {
        OnUpdateComplete?.Invoke();
      });

      return _underlying.Join();
    }

    public IBroadcastChannelResult Leave() {
      _underlying.Off("font:description");
      _underlying.Off("font:deletion");
      _underlying.Off("udpate:complete");
      return _underlying.Leave();
    }

    public IBroadcastResponse SendUpdateRequest() {
      return _underlying.Send("update:request", null);
    }
    #endregion

    #region delegates
    public delegate void FontDescriptionHandler(Payloads.FontDescription desc);
    public delegate void FontDeletionHandler(string uid);
    public delegate void UpdateCompleteHandler();
    #endregion

    #region events
    public event FontDescriptionHandler OnFontDescription;
    public event FontDeletionHandler OnFontDeletion;
    public event UpdateCompleteHandler OnUpdateComplete;
    #endregion
  }
}
