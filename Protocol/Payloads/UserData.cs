using Newtonsoft.Json;

namespace Protocol.Payloads {
  public class UserData {
    [JsonProperty("uid")]
    public string UID { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("last_name")]
    public string LastName { get; set; }
    [JsonProperty("account_url")]
    public string AccountUrl { get; set; }
    [JsonProperty("settings_url")]
    public string SettingsUrl { get; set; }
    [JsonProperty("reuse_token")]
    public string ReuseToken { get; set; }
  }
}
