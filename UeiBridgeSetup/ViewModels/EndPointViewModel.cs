using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UeiBridgeSetup.ViewModels
{
    public enum EndPointLocation { Local, Dest}
    public class EndPointViewModel : ViewModelBase
    {
        readonly EndPointLocation _epLocation;
        //private string _ipString;
        //private int _ipPort;

        private IPEndPoint EndPoint { get; set; }
        public string IpString
        {
            get => this.EndPoint.Address.ToString();// _ipString;
            //set
            //{
            //    _ipString = value;
                //RaisePropertyChanged();
            //}
        }
        public int IpPort 
        {
            get => this.EndPoint.Port;// _ipPort;
            //private set 
            //{ 
                //_ipPort = value;
                //RaisePropertyChanged();
            //}
        }

        //public string ip { get; set; }
        public string EndpointHeader
        {
            get
            {
                string h = (_epLocation == EndPointLocation.Local) ? "Local end point (Rx)" : "Destination end point (Tx)";
                return h;
            }
        }
        public EndPointViewModel(EndPointLocation epLocation, IPEndPoint endPoint = null)
        {
            _epLocation = epLocation;
            //SetEndPoint(endPoint);
            if (null == endPoint)
            {
                this.EndPoint = new IPEndPoint(IPAddress.Any, 0);
            }
            else
            {
                this.EndPoint = endPoint;
            }
        }
    }
}
