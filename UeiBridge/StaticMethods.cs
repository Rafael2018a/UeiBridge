using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

namespace UeiBridge
{
    static class StaticMethods
    {
        static string _lastErrorMessage;

        public static string LastErrorMessage { get => _lastErrorMessage; }

        public static List<Device> GetDeviceList()
        {
            DeviceCollection devColl = new DeviceCollection(Config.Instance.DeviceUrl);
            List<Device> resultList = new List<Device>();
            try
            {
                foreach (Device dev in devColl)
                {
                    if (dev != null)
                    {
                        resultList.Add(dev);
                    }
                }
            }
            catch (Exception ex)
            {
                _lastErrorMessage = ex.Message;
                return null;
            }
            return resultList;
        }
        /// <summary>
        /// Example: input "DO-403", output "Dev0"
        /// return null if device not found
        /// </summary>
        public static string FindDeviceIndex(string deviceName)
        {
            List<Device> devList = GetDeviceList();
            var x = devList.Find(s => s.GetDeviceName() == deviceName);
            if (null != x)
            {
                string rc = "Dev" + x.GetIndex() + "/";
                return rc;
            }
            else
                return null;

        }

        public static log4net.ILog GetLogger()
        {
            //var m = System.Reflection.MethodBase.GetCurrentMethod();
            //var m1 = System.Reflection.MethodBase.GetCurrentMethod().

            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
            // Get calling method name
            var m = stackTrace.GetFrame(1).GetMethod();


            var x = log4net.LogManager.GetLogger( m.DeclaringType.Name);
            return x;
        }
    }
}
