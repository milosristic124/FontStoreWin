using Newtonsoft.Json;

namespace Protocol.Payloads {
  public class Authentication {
    //[JsonProperty("login")]
    //public string Login { get; set; }
    //[JsonProperty("password")]
    //public string Password { get; set; }
    [JsonProperty("protocol_version")]
    public string ProtocolVersion { get; set; }
    [JsonProperty("application_version")]
    public string ApplicationVersion { get; set; }
    [JsonProperty("os")]
    public string Os { get; set; }
    [JsonProperty("os_version")]
    public string OsVersion { get; set; }
  }

  public class Connect: Authentication {
    [JsonProperty("login")]
    public string Login { get; set; }
    [JsonProperty("password")]
    public string Password { get; set; }
  }

  public class AutoConnect : Authentication {
    [JsonProperty("auth_token")]
    public string AuthToken { get; set; }
  }
}
