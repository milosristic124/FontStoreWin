using System.Net;

namespace Protocol.Transport.Http.Impl {
  public class HttpTransport : IHttpTransport {
    #region properties
    public int DownloadParallelism { get; set; }
    #endregion

    #region methods
    public IHttpRequest CreateHttpRequest(string endpoint) {
      HttpWebRequest request = WebRequest.CreateHttp(endpoint);
      return new HttpRequest(request);
    }
    #endregion
  }
}
