using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusViewer
{
    class SecondsToTimeConverter : System.Windows.Data.IValueConverter
    {
        DateTime now = DateTime.Now;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val = System.Convert.ToDouble(value);
            DateTime dt = new DateTime(now.Year, now.Month, now.Day).AddSeconds(val);
            return string.Format("{0:T}", dt) + "." + dt.Millisecond;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
