using Newtonsoft.Json;

namespace Protocol.Payloads {
  public class UpdateRequest {
    [JsonProperty("last_udpate_date")]
    public int LastUpdateDate { get; set; }
  }
}
