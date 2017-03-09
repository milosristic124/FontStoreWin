using System.IO;
using System.Net;
using System.Threading.Tasks;
using Utilities.Extensions;

namespace Protocol.Transport.Phoenix {
  public class HttpRequest : IHttpRequest {
    #region private data
    private HttpWebRequest _impl;
    #endregion

    #region properties
    public string ContentType {
      get {
        return _impl.ContentType;
      }

      set {
        _impl.ContentType = value;
      }
    }

    public string Method {
      get {
        return _impl.Method;
      }

      set {
        _impl.Method = value;
      }
    }

    public Stream RequestStream {
      get {
        return _impl.GetRequestStream();
      }
    }

    public Task<IHttpResponse> Response {
      get {
        return _impl.GetResponseAsync().Then<WebResponse, IHttpResponse>(response => {
          return new HttpResponse(response as HttpWebResponse);
        });
      }
    }

    public int Timeout {
      get {
        return _impl.Timeout;
      }

      set {
        _impl.Timeout = value;
      }
    }
    #endregion

    #region ctor
    internal HttpRequest(HttpWebRequest request) {
      _impl = request;
    }
    #endregion

    #region methods
    public void Abort() {
      _impl.Abort();
    }
    #endregion
  }
}
