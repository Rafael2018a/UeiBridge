using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UeiBridgeSetup.ViewModels
{
    enum EndPointLocation { Local, Dest}
    class EndPointViewModel: ViewModelBase
    {
        EndPointLocation _epLocation;

        public string BoxHeader 
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
        }
    }
}
