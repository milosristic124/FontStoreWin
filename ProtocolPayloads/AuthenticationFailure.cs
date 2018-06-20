using Newtonsoft.Json;

namespace Protocol.Payloads {
  public class AuthenticationFailure {
    [JsonProperty("message")]
    public string Message { get; set; }
  }
}
