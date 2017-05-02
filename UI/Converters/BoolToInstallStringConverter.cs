using System;
using System.Globalization;
using System.Windows.Data;

namespace UI.Converters {
  [ValueConversion(typeof(bool), typeof(string))]
  class BoolToInstallStringConverter : BaseConverter, IValueConverter {
    #region methods
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      bool installed = (bool)value;
      if (installed) {
        return "Uninstall";
      }
      return "Install";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return null;
    }
    #endregion
  }
}
