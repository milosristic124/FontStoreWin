using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UI.Converters {
  [ValueConversion(typeof(string), typeof(FontStyle))]
  class StringToFontStyleConverter : BaseConverter, IValueConverter {
    #region methods
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      string lowerStyle = ((string)value).ToLower();
      if (lowerStyle.Contains("italic")) {
        return FontStyles.Italic;
      } else if (lowerStyle.Contains("oblique")) {
        return FontStyles.Oblique;
      } else {
        return FontStyles.Normal;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return null;
    }
    #endregion
  }
}
