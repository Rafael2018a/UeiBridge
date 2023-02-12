using System;
using System.ComponentModel;
using System.Windows.Media;

namespace StatusViewer
{
    public class StatusBaseViewModel : INotifyPropertyChanged
    {
        // privates
        protected static bool _enableBindingUpdate = true;
        protected DateTime lastMidnight;
        //protected double _lastUpdateInSec;
        protected string _lastUpdate;
        protected string _desc;
        System.Windows.Media.Color _borderBrushColor;


        // publics (for bind)
        public string Desc => _desc;
        //public System.Windows.Media.Color BorderBrushColor => System.Windows.Media.Colors.RoyalBlue;
        //public double LastUpdateInSec
        //{
        //    get { return _lastUpdateInSec; }
        //    set
        //    {
        //        _lastUpdateInSec = value;
        //        RaisePropertyChangedEvent("LastUpdateInSec");
        //    }
        //}
        public string LastUpdate
        {
            get { return _lastUpdate; }
            set
            {
                _lastUpdate = value;
                RaisePropertyChangedEvent("LastUpdate");
            }
        }

        public Color BorderBrushColor
        {
            get => _borderBrushColor;
            set
            {
                _borderBrushColor = value;
                RaisePropertyChangedEvent("BorderBrushColor");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public StatusBaseViewModel(StatusEntryModel messageModel)
        {
            DateTime now = DateTime.Now;
            lastMidnight = new DateTime(now.Year, now.Month, now.Day);
            //_lastUpdateInSec = messageModel.ProjTimeInSec;
            _desc = messageModel.Desc;
        }
        protected void RaisePropertyChangedEvent(string eventName)
        {
            if (_enableBindingUpdate && PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(eventName));
        }
    }
}
