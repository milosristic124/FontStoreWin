using Newtonsoft.Json;

namespace Protocol.Payloads {
  public class FontDescription {
    [JsonProperty("uid")]
    public string UID { get; set; }
    [JsonProperty("font_style")]
    public string Style { get; set; }
    [JsonProperty("font_family")]
    public string FamilyName { get; set; }
    [JsonProperty("download_url")]
    public string DownloadUrl { get; set; }
    [JsonProperty("transmitted_at")]
    public int TransmittedAt { get; set; }
  }
}
