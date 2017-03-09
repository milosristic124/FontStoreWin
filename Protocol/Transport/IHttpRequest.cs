using System;
using System.IO;
using System.Threading.Tasks;

namespace Protocol.Transport {
  public interface IHttpRequest {
    string Method { get; set; }
    string ContentType { get; set; }
    int Timeout { get; set; }
    Stream RequestStream { get; }
    Task<IHttpResponse> Response { get; }

    void Abort();
  }
}
