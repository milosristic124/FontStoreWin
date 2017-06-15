using Newtonsoft.Json;

namespace Protocol.Payloads {
  public class UpdateComplete {
    [JsonProperty("transmitted_at")]
    public int TransmittedAt { get; set; }
  }
}
