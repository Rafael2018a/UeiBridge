using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.Library;

namespace UeiBridgeSetup.ViewModels
{
    public enum EndPointLocation { Local, Dest}
    public class EndPointViewModel : ViewModelBase
    {
        readonly EndPointLocation _epLocation;

        private UeiBridge.Library.EndPoint EndPoint { get; set; }
        public string IpString
        {
            get => this.EndPoint.Address.ToString();// _ipString;
        }
        public int IpPort 
        {
            get => this.EndPoint.Port;
            set
            {
                this.EndPoint.Port = value;
                RaisePropertyChanged();
            }
        }

        public bool IsVisible
        {
            get { return this.EndPoint.Port > 0; }
            set { }
        }
        public string EndpointHeader
        {
            get
            {
                string h = (_epLocation == EndPointLocation.Local) ? "Local end point (Rx)" : "Destination end point (Tx)";
                return h;
            }
        }
        public EndPointViewModel(EndPointLocation epLocation, UeiBridge.Library.EndPoint endPoint)
        {
            _epLocation = epLocation;

            if (null == endPoint)
            {
                this.EndPoint = new UeiBridge.Library.EndPoint("0.0.0.0", 0);
            }
            else
            {
                this.EndPoint = endPoint;
            }
        }
    }
}
