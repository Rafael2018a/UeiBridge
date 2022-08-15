using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusViewer
{
    class EnumToStringConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string enumString;
            try
            {
                enumString = Enum.GetName((value.GetType()), value);
            }
            catch (Exception ex)
            {
                enumString = ex.Message;
            }

            return enumString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
