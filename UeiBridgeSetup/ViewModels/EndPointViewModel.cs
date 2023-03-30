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

        EndPointLocation _epLocation;
        private string _ipString;
        private int _ipPort;

        IPEndPoint EndPoint { get; set; }
        public string IpString
        {
            get => _ipString;
            set
            {
                _ipString = value;
                RaisePropertyChanged();
            }
        }
        public int IpPort 
        { 
            get => _ipPort;
            set 
            { 
                _ipPort = value;
                RaisePropertyChanged();
            }
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
        public EndPointViewModel(EndPointLocation epLocation)
        {
            _epLocation = epLocation;
            //this.EndPoint = new IPEndPoint(IPAddress.Parse("5.5.5.5"), 555);
        }

        internal void SetEndPoint(IPEndPoint endPoint)
        {
            this.EndPoint = endPoint;
            if (EndPoint != null)
            {
                this.IpString = EndPoint.Address.ToString();
                this.IpPort = EndPoint.Port;
            }
            else
            {
                this.IpString = null;
                this.IpPort = 0;
            }
        }
    }
}
