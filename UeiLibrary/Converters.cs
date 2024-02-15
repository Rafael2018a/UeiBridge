using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

namespace UeiBridge.Library
{
    class Converters
    {
    }

    public interface IValueConverter< FromType, ToType>
    {
        //
        // Summary:
        //     Converts a value.
        //
        // Parameters:
        //   value:
        //     The value produced by the binding source.
        //
        //   targetType:
        //     The type of the binding target property.
        //
        //   parameter:
        //     The converter parameter to use.
        //
        //   culture:
        //     The culture to use in the converter.
        //
        // Returns:
        //     A converted value. If the method returns null, the valid null value is used.
        ToType Convert(FromType fromValue, object parameter);
        //
        // Summary:
        //     Converts a value.
        //
        // Parameters:
        //   value:
        //     The value that is produced by the binding target.
        //
        //   targetType:
        //     The type to convert to.
        //
        //   parameter:
        //     The converter parameter to use.
        //
        //   culture:
        //     The culture to use in the converter.
        //
        // Returns:
        //     A converted value. If the method returns null, the valid null value is used.
        FromType ConvertBack(ToType fromValue, object parameter);
    }

    public class SerialPortSpeedToInt : IValueConverter<UeiDaq.SerialPortSpeed, int>
    {
        public int Convert(SerialPortSpeed fromValue, object parameter)
        {
            throw new NotImplementedException();
        }

        public SerialPortSpeed ConvertBack(int fromValue, object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
