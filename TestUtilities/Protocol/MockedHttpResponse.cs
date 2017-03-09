using Protocol.Transport;
using System.IO;
using System.Net;
using System.Text;

namespace TestUtilities.Protocol {
  public class MockedHttpResponse : IHttpResponse {
    #region private data
    private string _body;
    #endregion

    #region properties
    public Stream ResponseStream {
      get {
        return new MemoryStream(Encoding.UTF8.GetBytes(_body.ToCharArray()));
      }
    }

    public HttpStatusCode StatusCode { get; private set; }
    #endregion

    #region ctor
    internal MockedHttpResponse(HttpStatusCode status, string body) {
      StatusCode = status;
      _body = body;
    }
    #endregion

    #region IDisposable
    public void Dispose() {
    }
    #endregion
  }
}
