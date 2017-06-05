using Newtonsoft.Json;

namespace Protocol.Payloads {
  public class UpdateRequest {
    [JsonProperty("last_update_date")]
    public string LastUpdateDate { get; set; }
  }
}
