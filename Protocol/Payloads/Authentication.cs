using Newtonsoft.Json;

namespace Protocol.Payloads {
  class Authentication {
    [JsonProperty("login")]
    public string Login { get; set; }
    [JsonProperty("password")]
    public string Password { get; set; }
    [JsonProperty("protocol_version")]
    public int ProtocolVersion { get; set; }
    [JsonProperty("application_version")]
    public string ApplicationVersion { get; set; }
    [JsonProperty("os")]
    public string Os { get; set; }
    [JsonProperty("os_version")]
    public string OsVersion { get; set; }
  }
}
