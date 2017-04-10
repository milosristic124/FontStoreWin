using System.IO;
using System.Text;

namespace Encryption.Impl {
  public class XORCypher: ICypher {
    #region private data
    private byte[] _bKey;
    #endregion

    #region properties
    public string Key { get; private set; }
    #endregion

    #region ctor
    public XORCypher(string key) {
      Key = key;
      _bKey = Encoding.UTF8.GetBytes(Key);
    }
    #endregion

    #region methods
    public MemoryStream Decrypt(Stream data) {
      MemoryStream result = new MemoryStream();

      using (data) {
        int readCount = 0;
        byte[] buffer = new byte[_bKey.Length];
        do {
          readCount = data.Read(buffer, 0, buffer.Length);
          byte[] decrypted = Decrypt(buffer, readCount, _bKey);
          result.Write(decrypted, 0, decrypted.Length);
        } while (readCount > 0);
      }

      result.Seek(0, SeekOrigin.Begin);
        return result;
    }
    #endregion

    #region private methods
    private byte[] Decrypt(byte[] buffer, int count, byte[] key) {
      byte[] result = new byte[count];

      for (int it = 0; it < count; it++) {
        result[it] = (byte)(buffer[it] ^ key[it & key.Length]);
      }

      return buffer;
    }
    #endregion
  }
}
