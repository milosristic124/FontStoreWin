using Protocol.Transport.Http;

namespace TestUtilities.Protocol {
  public class MockedHttpTransport: CallTracer, IHttpTransport {
    #region properties
    public int DownloadParallelism { get; set; }
    #endregion

    #region ctor
    public MockedHttpTransport() {
      DownloadParallelism = 2;
    }
    #endregion

    #region methods
    public IHttpRequest CreateHttpRequest(string endpoint) {
      RegisterCall("CreateHttpRequest");
      MockedHttpRequest request = new MockedHttpRequest(endpoint);
      request.OnRequestSent += Request_OnRequestSent;
      return request;
    }
    #endregion

    #region test
    public delegate MockedHttpResponse HttpRequestSentHandler(MockedHttpRequest request, string body);
    public event HttpRequestSentHandler OnHttpRequestSent;
    #endregion

    #region private methods
    private MockedHttpResponse Request_OnRequestSent(MockedHttpRequest request, string body) {
      return OnHttpRequestSent?.Invoke(request, body);
    }
    #endregion
  }
}
