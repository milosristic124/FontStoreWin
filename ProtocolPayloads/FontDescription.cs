using Newtonsoft.Json;

namespace Protocol.Payloads {
  public class FontDescription {
    [JsonProperty("uid")]
    public string UID { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("family_name")]
    public string FamilyName { get; set; }
    [JsonProperty("download_url")]
    public string DownloadUrl { get; set; }
    [JsonProperty("created_at")]
    public int CreatedAt { get; set; }
  }
}
