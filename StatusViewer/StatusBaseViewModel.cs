using System;
using System.ComponentModel;

namespace StatusViewer
{
    public class StatusBaseViewModel :INotifyPropertyChanged
    {
        protected static bool _enableBindingUpdate = true;
        protected DateTime lastMidnight;
        protected double _lastUpdateInSec;

        protected string _lastUpdate;
        //public string Desc { get; set; }
        protected string _desc;
        public string Desc => _desc;

        public double LastUpdateInSec
        {
            get { return _lastUpdateInSec; }
            set
            {
                _lastUpdateInSec = value;
                RaisePropertyChangedEvent("LastUpdateInSec");
            }
        }
        public string LastUpdate
        {
            get { return _lastUpdate; }
            set
            {
                _lastUpdate = value;
                RaisePropertyChangedEvent("LastUpdate");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public StatusBaseViewModel(ProjMessageModel messageModel)
        {
            DateTime now = DateTime.Now;
            lastMidnight = new DateTime(now.Year, now.Month, now.Day);
            _lastUpdateInSec = messageModel.ProjTimeInSec;
            _desc = messageModel.Desc;
        }
        protected void RaisePropertyChangedEvent(string eventName)
        {
            if (_enableBindingUpdate && PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(eventName));
        }
    }
}
