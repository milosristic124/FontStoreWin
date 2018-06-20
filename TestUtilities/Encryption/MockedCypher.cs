using Encryption;
using System;
using System.IO;

namespace TestUtilities.Encryption {
  public class MockedCypher : ICypher {
    public string Key { get; }

    public MemoryStream Decrypt(Stream data) {
      MemoryStream res = new MemoryStream();
      data.CopyTo(res);
      res.Seek(0, SeekOrigin.Begin);
      return res;
    }
  }
}
