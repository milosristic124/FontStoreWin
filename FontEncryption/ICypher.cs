using System.IO;

namespace Encryption {
  public interface ICypher {
    string Key { get; }
    MemoryStream Decrypt(Stream data);
  }
}
