using System;
using System.IO;

namespace Logging {
  class LogStorage {
    #region data
    private StreamWriter _writer;
    #endregion

    #region ctor
    public LogStorage() {
      string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Fontstore";
      Directory.CreateDirectory(root);

      string logFile = root + "/fontstore.log";
      _writer = new StreamWriter(logFile, false);
    }
    #endregion

    #region methods
    public void SaveLog(string msg) {
      _writer.WriteLine(msg);
      _writer.Flush();
    }
    #endregion
  }

  public class Logger
  {
    #region static data
    private static LogStorage _logStorage = null;
    #endregion

    #region ctor
    private Logger() {
      throw new InvalidOperationException("Logger.Initialize must be used to instanciate the logger");
    }
    #endregion

    #region static methods
    public static void Initialize() {
      if (_logStorage == null) {
        _logStorage = new LogStorage();
        Log("Logger initialized");
      }
    }

    public static void Log(string format, params object[] args) {
      if (_logStorage != null) {
        string msg = string.Format(format, args);
        string formattedMsg = string.Format("[{0}] {1}", DateTime.Now.ToString("hh:mm:ss.fff"), msg);
        Console.WriteLine(formattedMsg);
        _logStorage.SaveLog(formattedMsg);
      } else {
        throw new InvalidOperationException("Logger.Log can only be called after Logger.Initialize has been called");
      }
    }
    #endregion
  }
}
