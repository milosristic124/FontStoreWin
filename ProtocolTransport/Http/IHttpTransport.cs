namespace Protocol.Transport.Http {
  public interface IHttpTransport {
    #region properties
    int DownloadParallelism { get; set; }
    #endregion

    #region methods
    IHttpRequest CreateHttpRequest(string endpoint);
    #endregion
  }
}
