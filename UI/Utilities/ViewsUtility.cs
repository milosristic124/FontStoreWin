using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.Utilities {
  static class ViewsUtility {
    public static void NavigateToUri(Uri uri) {
      Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
    }
  }
}
