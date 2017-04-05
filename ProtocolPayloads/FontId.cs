using Newtonsoft.Json;

namespace Protocol.Payloads {
  public class FontId {
    [JsonProperty("uid")]
    public string UID { get; set; }
    [JsonProperty("transmitted_at")]
    public int TransmittedAt { get; set; }
  }
}
