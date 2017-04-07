using Newtonsoft.Json;

namespace Protocol.Payloads {
  public class FontId {
    [JsonProperty("uid")]
    public string UID { get; set; }
  }
}
