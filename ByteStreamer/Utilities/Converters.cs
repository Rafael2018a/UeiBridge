using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteStreamer.Utilities
{
    /// <summary>
    /// invert boolean values
    /// </summary>
    public class BoolConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bVal = System.Convert.ToBoolean(value);
            return (bVal == true) ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bVal = System.Convert.ToBoolean(value);
            return (bVal == true) ? false : true;
        }
    }
}
