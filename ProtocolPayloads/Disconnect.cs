using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Payloads {
  public class Disconnect {
    [JsonProperty("reason")]
    public string Reason { get; set; }
  }
}
