using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace UeiBridge.CubeNet
{
    class Converters
    {
    }
    public class IpAddressToStringConverter : System.Windows.Data.IValueConverter //RadioBoolToIntConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IPAddress ip = value as IPAddress;
            //System.Diagnostics.Debug.Assert(null != ip);
            if (null == ip)
            {
                return "";
            }
            else
            {
                return ip.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string ipstring = value as string;
            System.Diagnostics.Debug.Assert(null != ipstring);
            IPAddress ip;
            if (IPAddress.TryParse(ipstring, out ip))
            {
                return ip;
            }
            else
            {
                return null;
            }

        }
    }

}
