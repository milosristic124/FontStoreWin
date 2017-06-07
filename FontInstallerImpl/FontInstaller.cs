using Logging;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FontInstaller.Impl {
  public class FontInstaller : IFontInstaller {
    #region private data
    private Dictionary<string, string> _userFonts;
    private Dictionary<string, string> _privateFonts;

    private string _userFilesDir;
    private string _privateFilesDir;
    #endregion

    #region ctor
    public FontInstaller() {
      _userFonts = new Dictionary<string, string>();
      _privateFonts = new Dictionary<string, string>();

      _userFilesDir = Path.GetTempPath() + Guid.NewGuid().ToString() + "\\";
      Directory.CreateDirectory(_userFilesDir);
      Logger.Log("User font directory: {0}", _userFilesDir);

      _privateFilesDir = Path.GetTempPath() + Guid.NewGuid().ToString() + "\\";
      Directory.CreateDirectory(_privateFilesDir);
      Logger.Log("Process font directory: {0}", _privateFilesDir);
    }
    #endregion

    #region methods
    public InstallationScope GetFontInstallationScope(string uid) {
      bool privateScope = _privateFonts.ContainsKey(uid);
      bool userScope = _userFonts.ContainsKey(uid);

      InstallationScope scope = InstallationScope.None;
      if (_privateFonts.ContainsKey(uid)) {
        scope |= InstallationScope.Process;
      }
      if (_userFonts.ContainsKey(uid)) {
        scope |= InstallationScope.User;
      }
      return scope;
    }

    public async Task<FontAPIResult> InstallFont(string uid, InstallationScope scope, MemoryStream fontData) {
      return await Task.Run(delegate {
        switch (scope) {
          case InstallationScope.Process:
            return InstallPrivateFont(uid, fontData);

          case InstallationScope.User:
            return InstallUserFont(uid, fontData);

          default: return FontAPIResult.Failure;
        }
      });
    }

    public async Task<FontAPIResult> UninstallFont(string uid, InstallationScope scope) {
      return await Task.Run(delegate {
        switch (scope) {
          case InstallationScope.Process:
            return UninstallPrivateFont(uid);

          case InstallationScope.User:
            return UninstallUserFont(uid);

          default: return FontAPIResult.Failure;
        }
      });
    }

    public async Task UninstallAllFonts() {
      await Task.Run(delegate {
        try {
          Logger.Log("Uninstalling user fonts");
          foreach (string path in Directory.EnumerateFiles(_userFilesDir)) {
            RemoveUserFont(path);
          }

          Logger.Log("Uninstalling process fonts");
          foreach (string path in Directory.EnumerateFiles(_privateFilesDir)) {
            RemovePocessFont(path);
          }
        }
        catch (Exception) { }
        _userFonts.Clear();
        _privateFonts.Clear();
        Logger.Log("Uninstalling fonts done");
      });
    }
    #endregion

    #region private methods
    private string TempUserFilePath() {
      return _userFilesDir + Guid.NewGuid().ToString();
    }

    private string TempProcessFilePath() {
      return _privateFilesDir + Guid.NewGuid().ToString();
    }

    private FontAPIResult InstallPrivateFont(string uid, MemoryStream data) {
      if (_privateFonts.ContainsKey(uid)) {
        return FontAPIResult.Noop;
      }
      else {
        using (data) {
          data.Seek(0, SeekOrigin.Begin);
          string tempFilePath = TempProcessFilePath();
          try {
            using (FileStream fileStream = File.Create(tempFilePath)) {
              data.CopyTo(fileStream);
            }
            bool activatedFonts = AddFontResourceEx(tempFilePath, FR_PRIVATE, IntPtr.Zero) != 0;

            if (activatedFonts) {
              _privateFonts[uid] = tempFilePath;
              SendNotifyMessage(HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
              return FontAPIResult.Success;
            }

            return FontAPIResult.Failure;
          }
          catch (Exception) {
            if (File.Exists(tempFilePath)) {
              File.Delete(tempFilePath);
            }
          }

          return FontAPIResult.Failure;
        }
      }
    }

    private FontAPIResult InstallUserFont(string uid, MemoryStream data) {
      if (_userFonts.ContainsKey(uid)) {
        return FontAPIResult.Noop;
      }
      else {
        using (data) {
          data.Seek(0, SeekOrigin.Begin);
          string tempFilePath = TempUserFilePath();
          try {
            using (FileStream fileStream = File.Create(tempFilePath)) {
              data.CopyTo(fileStream);
            }
            bool activatedFonts = AddFontResource(tempFilePath) != 0;

            if (activatedFonts) {
              _userFonts[uid] = tempFilePath;
              SendNotifyMessage(HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
              return FontAPIResult.Success;
            }

            return FontAPIResult.Failure;
          }
          catch (Exception) {
            if (File.Exists(tempFilePath)) {
              File.Delete(tempFilePath);
            }
          }

          return FontAPIResult.Failure;
        }
      }
    }

    private FontAPIResult UninstallPrivateFont(string uid) {
      if (!_privateFonts.ContainsKey(uid)) {
        return FontAPIResult.Noop;
      }
      else {
        string fontFilePath = _privateFonts[uid];
        if (RemovePocessFont(fontFilePath)) {
          _privateFonts.Remove(uid);
          return FontAPIResult.Success;
        }

        return FontAPIResult.Failure;
      }
    }

    private FontAPIResult UninstallUserFont(string uid) {
      if (!_userFonts.ContainsKey(uid)) {
        return FontAPIResult.Noop;
      } else {
        string fontFilePath = _userFonts[uid];
        if (RemoveUserFont(fontFilePath)) {
          _userFonts.Remove(uid);
          return FontAPIResult.Success;
        }

        return FontAPIResult.Failure;
      }
    }
    #endregion

    #region font API encapsulation
    private bool RemovePocessFont(string path) {
      if (File.Exists(path)) {
        int attempt = 0;
        while (RemoveFontResourceEx(path, FR_PRIVATE, IntPtr.Zero) != 0) {
          SendNotifyMessage(HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
          attempt += 1;
        }

        File.Delete(path);
        return attempt > 0;
      }
      return false;
    }

    private bool RemoveUserFont(string path) {
      if (File.Exists(path)) {
        int attempt = 0;
        while (RemoveFontResource(path) != 0) {
          SendNotifyMessage(HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
          attempt += 1;
        }
        File.Delete(path);
        return attempt > 0;
      }
      return false;
    }
    #endregion

    #region font API
    [DllImport("gdi32.dll")]
    private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
            IntPtr pdv, [In] ref uint pcFonts);

    [DllImport("gdi32.dll")]
    private static extern int RemoveFontMemResourceEx([In] IntPtr fh);


    [DllImport("gdi32.dll", EntryPoint = "AddFontResource")]
    private static extern int AddFontResource(string lpFileName);

    [DllImport("gdi32.dll", EntryPoint = "AddFontResourceEx")]
    private static extern int AddFontResourceEx(string lpFileName, [In] uint fl, [In] IntPtr pdv);

    [DllImport("gdi32.dll", EntryPoint = "RemoveFontResource")]
    private static extern int RemoveFontResource(string lpFileName);

    [DllImport("gdi32.dll", EntryPoint = "RemoveFontResourceEx")]
    private static extern int RemoveFontResourceEx(string lpFileName, [In] uint fl, [In] IntPtr pdv);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    private const uint WM_FONTCHANGE = 0x001D;
    private const uint FR_PRIVATE = 0x10;
    private const uint FS_NOT_ENUM = 0x20;
    private IntPtr HWND_BROADCAST = new IntPtr(0xffff);
    #endregion
  }
}
