using Newtonsoft.Json;

namespace Protocol.Payloads {
  public class Disconnect {
    [JsonProperty("reason")]
    public string Reason { get; set; }
  }
}
