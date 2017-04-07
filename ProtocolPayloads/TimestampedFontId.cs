using Newtonsoft.Json;

namespace Protocol.Payloads {
  public class TimestampedFontId: FontId {
    [JsonProperty("transmitted_at")]
    public int TransmittedAt { get; set; }
  }
}
