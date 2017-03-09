using System.IO;
using System.Net;

namespace Protocol.Transport.Phoenix {
  public class HttpResponse : IHttpResponse {
    #region private data
    private HttpWebResponse _impl;
    #endregion

    #region properties
    public Stream ResponseStream {
      get {
        return _impl.GetResponseStream();
      }
    }

    public HttpStatusCode StatusCode {
      get {
        return _impl.StatusCode;
      }
    }
    #endregion

    #region ctor
    internal HttpResponse(HttpWebResponse response) {
      _impl = response;
    }
    #endregion

    #region IDisposable Support
    public void Dispose() {
      _impl.Dispose();
    }
    #endregion
  }
}
