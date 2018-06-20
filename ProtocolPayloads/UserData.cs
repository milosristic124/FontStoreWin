using Newtonsoft.Json;

namespace Protocol.Payloads {
  public class UserData {
    [JsonProperty("uid")]
    public string UID { get; set; }
    [JsonProperty("first_name")]
    public string FirstName { get; set; }
    [JsonProperty("last_name")]
    public string LastName { get; set; }
    [JsonProperty("reusable_token")]
    public string AuthToken { get; set; }

    [JsonProperty("urls")]
    public UserUrls Urls { get; set; }
  }

  public class UserUrls {
    [JsonProperty("account")]
    public string Account { get; set; }
    [JsonProperty("settings")]
    public string Settings { get; set; }
    [JsonProperty("visit")]
    public string Visit { get; set; }
  }
}
