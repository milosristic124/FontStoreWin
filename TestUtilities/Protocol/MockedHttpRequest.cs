using Protocol.Transport;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestUtilities.Protocol {
  public class MockedHttpRequest : IHttpRequest {
    #region private data
    private MemoryStream _bodyStream;
    #endregion

    #region properties
    public string ContentType { get; set; }
    public string Endpoint { get; private set; }
    public string Method { get; set; }

    public Stream RequestStream {
      get {
        return _bodyStream;
      }
    }

    public Task<IHttpResponse> Response {
      get {
        return Task.Factory.StartNew<IHttpResponse>(() => {
          string body = Encoding.UTF8.GetString(_bodyStream.ToArray());
          MockedHttpResponse response = OnRequestSent?.Invoke(this, body);
          if (response != null) {
            return response;
          }

          throw new WebException(string.Format("No response provided for http request - {0}:{1}", Method, Endpoint));
        });
      }
    }

    public int Timeout { get; set; }
    #endregion

    #region ctor
    internal MockedHttpRequest(string endpoint) {
      Endpoint = endpoint;
      _bodyStream = new MemoryStream();
    }
    #endregion

    #region methods
    public void Abort() {
      throw new NotImplementedException();
    }
    #endregion

    #region test methods
    public delegate MockedHttpResponse RequestSentHandler(MockedHttpRequest request, string body);
    public event RequestSentHandler OnRequestSent;

    public MockedHttpResponse CreateResponse(HttpStatusCode status, string body = null) {
      return new MockedHttpResponse(status, body);
    }
    #endregion
  }
}
