using System;
using System.Globalization;
using System.Windows.Data;

namespace UI.Converters {
  [ValueConversion(typeof(string), typeof(string))]
  class StringToFontWeightConverter : BaseConverter, IValueConverter {
    #region methods
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      string style = (string)value;
      string lowerStyle = style.ToLower();

      int italicIndex = lowerStyle.IndexOf("italic");
      string res = style;
      if (italicIndex < 0) {
        return style.Trim();
      } else if (italicIndex == 0) {
        return "Regular";
      } else {
        return style.Remove(italicIndex).Trim();
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return null;
    }
    #endregion
  }
}
