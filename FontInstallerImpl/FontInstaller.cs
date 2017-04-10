using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FontInstaller.Impl {
  public class FontInstaller : IFontInstaller {
    #region private data
    private Dictionary<string, IntPtr> _privateFonts;
    private Dictionary<string, string> _userFonts;

    private string _userFilesDir;
    #endregion

    #region ctor
    public FontInstaller() {
      _privateFonts = new Dictionary<string, IntPtr>();
      _userFonts = new Dictionary<string, string>();

      _userFilesDir = Path.GetTempPath() + Guid.NewGuid().ToString() + "\\";
      Directory.CreateDirectory(_userFilesDir);
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
          foreach (string path in Directory.EnumerateFiles(_userFilesDir)) {
            RemoveUserFont(path);
          }
        }
        catch (Exception) { }
        _userFonts.Clear();
      });
    }
    #endregion

    #region private methods
    private string TempFilePath() {
      return _userFilesDir + Guid.NewGuid().ToString();
    }

    private FontAPIResult InstallPrivateFont(string uid, MemoryStream data) {
      if (_privateFonts.ContainsKey(uid)) {
        return FontAPIResult.Noop;
      }
      else {
        using (data) {
          data.Seek(0, SeekOrigin.Begin);
          byte[] bytes = data.ToArray();
          IntPtr fontPtr = Marshal.AllocCoTaskMem(bytes.Length);
          Marshal.Copy(bytes, 0, fontPtr, bytes.Length);
          uint dummy = 0;
          IntPtr handle = AddFontMemResourceEx(fontPtr, (uint)bytes.Length, IntPtr.Zero, ref dummy);
          Marshal.FreeCoTaskMem(fontPtr);

          if (handle == IntPtr.Zero) {
            return FontAPIResult.Failure;
          }
          else {
            _privateFonts.Add(uid, handle);
            SendNotifyMessage(HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
            return FontAPIResult.Success;
          }
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
          string tempFilePath = TempFilePath();
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
            File.Delete(tempFilePath);
          }

          return FontAPIResult.Failure;
        }
      }
    }

    private FontAPIResult UninstallPrivateFont(string uid) {
      if (!_privateFonts.ContainsKey(uid)) {
        return FontAPIResult.Noop;
      } else if (RemovePocessFont(_privateFonts[uid])) {
        _privateFonts.Remove(uid);
        return FontAPIResult.Success;
      }
      return FontAPIResult.Failure;
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
    private bool RemovePocessFont(IntPtr handle) {
      if (RemoveFontMemResourceEx(handle) != 0) {
        SendNotifyMessage(HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
        return true;
      }
      return false;
    }

    private bool RemoveUserFont(string path) {
      if (RemoveFontResource(path) != 0) {
        File.Delete(path);
        SendNotifyMessage(HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
        return true;
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

    [DllImport("gdi32.dll", EntryPoint = "RemoveFontResource")]
    private static extern int RemoveFontResource(string lpFileName);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    private const uint WM_FONTCHANGE = 0x001D;
    private IntPtr HWND_BROADCAST = new IntPtr(0xffff);
    #endregion
  }
}
