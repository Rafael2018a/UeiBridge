using System;
using System.Diagnostics;
using UeiBridge.Library;

namespace StatusViewer
{
    public enum ProjMessageType { Counter = 0, SimpleLog =1,  Text=2, Invalid};
    public class StatusEntryModel
    {
        //int _severity;
        ProjMessageType _messageType;
        //Int64 _int64value;
        string [] _stringValue;
        //double _projTimeInSec;
        //double fracFactor = Math.Pow(2, 32) - 1; 
        string _desc;
        StatusTrait _trait;

        [Obsolete]
        public ProjMessageType MessageType { get => _messageType; }
        //[Obsolete]
        //public long Int64value { get => _int64value; }
        public string [] StringValue { get => _stringValue; }
        //[Obsolete]
        //public double ProjTimeInSec { get => _projTimeInSec; }
        public string Desc { get => _desc; }
        public StatusTrait Trait { get => _trait; }


        public StatusEntryModel(StatusEntryJson js)
        {
            //this._jsonMessage = js;
            _messageType = ProjMessageType.Text; // not is use
            _desc = js.FieldTitle;
            _stringValue = js.FormattedStatus;
            _trait = js.Trait;
        }
    }
}
