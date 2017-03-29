using System;
using System.Collections.Generic;

namespace Protocol.Transport {
  public sealed class ResponseStatus {
    #region private data
    private static readonly Dictionary<string, ResponseStatus> _instances = new Dictionary<string, ResponseStatus>();
    #endregion

    #region properties
    public string Name { get; private set; }
    #endregion

    #region consts
    public static readonly ResponseStatus Ok = new ResponseStatus("ok");
    public static readonly ResponseStatus Error = new ResponseStatus("error");
    #endregion

    #region ctor
    private ResponseStatus(string name) {
      Name = name;
      _instances[name] = this;
    }
    #endregion

    #region methods
    public override string ToString() {
      return Name;
    }

    public static explicit operator ResponseStatus(string str) {
      ResponseStatus result;

      if (_instances.TryGetValue(str, out result)) {
        return result;
      } else {
        throw new InvalidCastException();
      }
    }
    #endregion
  }
}
