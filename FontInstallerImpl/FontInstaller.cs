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
    #endregion

    #region ctor
    public FontInstaller() {
      _privateFonts = new Dictionary<string, IntPtr>();
      _userFonts = new Dictionary<string, string>();
    }
    #endregion

    #region methods
    public async Task<bool> InstallFont(string uid, InstallationScope scope, MemoryStream fontData) {
      return await Task.Run(delegate {
        switch (scope) {
          case InstallationScope.Process:
            return InstallPrivateFont(uid, fontData);

          case InstallationScope.User:
            return InstallUserFont(uid, fontData);

          case InstallationScope.All:
            return InstallPrivateFont(uid, fontData) && InstallUserFont(uid, fontData);

          default: return false;
        }
      });
    }

    public async Task<bool> UnsintallFont(string uid, InstallationScope scope) {
      return await Task.Run(delegate {
        switch (scope) {
          case InstallationScope.Process:
            return UninstallPrivateFont(uid);

          case InstallationScope.User:
            return UninstallUserFont(uid);

          case InstallationScope.All:
            return UninstallPrivateFont(uid) && UninstallUserFont(uid);

          default: return false;
        }
      });
    }
    #endregion

    #region private methods
    private bool InstallPrivateFont(string uid, MemoryStream data) {
      if (_privateFonts.ContainsKey(uid)) {
        return true;
      }
      else {
        byte[] bytes = data.ToArray();
        IntPtr fontPtr = Marshal.AllocCoTaskMem(bytes.Length);
        Marshal.Copy(bytes, 0, fontPtr, bytes.Length);
        uint dummy = 0;
        IntPtr handle = AddFontMemResourceEx(fontPtr, (uint)bytes.Length, IntPtr.Zero, ref dummy);
        Marshal.FreeCoTaskMem(fontPtr);

        if (handle == IntPtr.Zero) {
          return false;
        }
        else {
          _privateFonts.Add(uid, handle);
          SendNotifyMessage(HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
          return true;
        }
      }
    }

    private bool InstallUserFont(string uid, MemoryStream data) {
      if (_userFonts.ContainsKey(uid)) {
        return true;
      } else {
        string tempFilePath = Path.GetTempPath() + Guid.NewGuid().ToString();
        try {
          using (FileStream fileStream = File.Create(tempFilePath)) {
            data.CopyTo(fileStream);
          }
          bool activatedFonts = AddFontResource(tempFilePath) != 0;

          if (activatedFonts) {
            SendNotifyMessage(HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
          }

          return activatedFonts;
        } catch (Exception) {
          File.Delete(tempFilePath);
        }

        return false;
      }
    }

    private bool UninstallPrivateFont(string uid) {
      if (!_privateFonts.ContainsKey(uid)) {
        return true;
      } else {
        if (RemoveFontMemResourceEx(_privateFonts[uid]) != 0) {
          _privateFonts.Remove(uid);
          SendNotifyMessage(HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
          return true;
        }
        return false;
      }
    }

    private bool UninstallUserFont(string uid) {
      if (!_userFonts.ContainsKey(uid)) {
        return true;
      } else {
        string fontFilePath = _userFonts[uid];
        if (RemoveFontResource(fontFilePath) != 0) {
          _userFonts.Remove(uid);
          File.Delete(fontFilePath);
          SendNotifyMessage(HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
          return true;
        }

        return false;
      }
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
