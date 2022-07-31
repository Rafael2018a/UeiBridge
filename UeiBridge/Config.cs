using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UeiBridge
{
    class Config
    {
        static Config _instance = new Config();
        readonly int _localPort = 50035;
        readonly string _deviceUrl = "pdna://192.168.100.2/";
        readonly Tuple<double, double> _analogOutMinMaxVoltage = new Tuple<double, double>( -10.0, 10.0 );
        readonly Tuple<double, double> _analogInMinMaxVoltage = new Tuple<double, double>(-15.0, 15.0); // -15,15 means 'no gain'
        readonly string _receiverMulticastAddress = "227.3.1.10";
        readonly string _destMulticastAddress = "227.2.1.10";
        readonly int _destMulticastPort = 50038;
        public Config()
        {
            
        }
        internal static Config Instance { get => _instance; }
        public int LocalPort => _localPort; 
        public string DeviceUrl => _deviceUrl;
        public Tuple<double, double> AnalogOutMinMaxVoltage => _analogOutMinMaxVoltage;
        public Tuple<double, double> AnalogInMinMaxVoltage => _analogInMinMaxVoltage;
        public string ReceiverMulticastAddress => _receiverMulticastAddress;
        public string DestMulticastAddress => _destMulticastAddress;
        public int DestMulticastPort => _destMulticastPort;
    }
}

 
