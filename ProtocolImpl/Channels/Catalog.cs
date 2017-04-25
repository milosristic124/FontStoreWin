using Protocol.Transport;
using System;
using Utilities;

namespace Protocol.Impl.Channels {
  class Catalog {
    #region private data
    private IConnection _connection;
    private IBroadcastChannel _underlying;
    #endregion

    #region properties
    public bool IsJoined {
      get { return _underlying.IsJoined; }
    }
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
      _underlying.On("font:deletion", (Payloads.TimestampedFontId del) => {
        OnFontDeletion?.Invoke(del);
      });
      _underlying.On("update:complete", () => {
        OnUpdateComplete?.Invoke();
      });
      _underlying.On("fonts-package", () => {
        OnNewFontReleased?.Invoke();
      });

      return _underlying.Join();
    }

    public IBroadcastChannelResult Leave() {
      _underlying.Off("font:description");
      _underlying.Off("font:deletion");
      _underlying.Off("udpate:complete");
      return _underlying.Leave();
    }

    public void SendUpdateRequest(DateTime? lastUpdate) {
      Payloads.UpdateRequest payload;
      if (lastUpdate == null) {
        payload = null;
      }
      else {
        payload = new Payloads.UpdateRequest() { LastUpdateDate = lastUpdate.Value.ToTimestamp() };
      }

      _underlying.Send("update:request", payload);
    }
    #endregion

    #region delegates
    public delegate void FontDescriptionHandler(Payloads.FontDescription desc);
    public delegate void FontDeletionHandler(Payloads.TimestampedFontId fid);
    public delegate void UpdateCompleteHandler();
    public delegate void FontReleasedHandler();
    #endregion

    #region events
    public event FontDescriptionHandler OnFontDescription;
    public event FontDeletionHandler OnFontDeletion;
    public event UpdateCompleteHandler OnUpdateComplete;
    public event FontReleasedHandler OnNewFontReleased;
    #endregion
  }
}
