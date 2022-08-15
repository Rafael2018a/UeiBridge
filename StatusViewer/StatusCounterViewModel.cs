using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusViewer
{
    public class StatusCounterViewModel: StatusBaseViewModel
    {
        long _incomingValue = 0;
        long _sum;
        double _rate;
        ProjMessageModel prevMessageModel;

        public long IncomingValue
        {
            get { return _incomingValue;}
            set
            {
                _incomingValue = value;
                RaisePropertyChangedEvent("IncomingValue");
            }
        }
        public double Rate
        {
            get{ return _rate;}
            set
            {
                _rate = value;
                RaisePropertyChangedEvent("Rate");
            }
        }
        public bool IsRateMeasure { get; set; } // tbd. 2 del
        public long Sum
        {
            get{return _sum;}
            set
            {
                _sum = value;
                RaisePropertyChangedEvent("Sum");
            }
        }

        public bool IsSumming { get; set; } // tbd. 2 del

        public static bool EnableBindingUpdate { get; set; }
        public StatusCounterViewModel(ProjMessageModel messageModel) : base(messageModel)
        {
            IncomingValue = messageModel.Int64value;
            //Sum = messageModel.Int64value;
            Rate = 0;
            IsSumming = true;
            IsRateMeasure = true;
            
            //_IsSumming = (receiveBuffer[3] & 0x01) > 0;
            //_IsRateMeasure = (receiveBuffer[3] & 0x02) > 0;
        }

        public void Update( ProjMessageModel messageModel)
        {
            IncomingValue = messageModel.Int64value;
            //Sum += messageModel.Int64value;
            double timeDiff = 0;
            if (prevMessageModel != null)
            {
                timeDiff = messageModel.ProjTimeInSec - prevMessageModel.ProjTimeInSec;
            };

            if (timeDiff > 0)
            {
                Rate = (double)(messageModel.Int64value - prevMessageModel.Int64value) / timeDiff;
            }

            LastUpdate = System.DateTime.Now.ToLongTimeString();
            prevMessageModel = messageModel;
        }

    }
}
