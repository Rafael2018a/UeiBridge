using System;
using System.Diagnostics;
using UeiLibrary;

namespace StatusViewer
{
    public enum ProjMessageType { Counter = 0, SimpleLog =1,  Text=2, Invalid};
    public class ProjMessageModel
    {
        int _severity;
        ProjMessageType _messageType;
        Int64 _int64value;
        string _stringValue;
        double _projTimeInSec;
        double fracFactor = Math.Pow(2, 32) - 1; // tbd. not sure about -1
        private string _desc;
        //private JsonStatusClass _jsonMessage;

        public int Severity { get => _severity; }
        public ProjMessageType MessageType { get => _messageType; }
        public long Int64value { get => _int64value; }
        public string StringValue { get => _stringValue; }
        public double ProjTimeInSec { get => _projTimeInSec; }
        public string Desc { get => _desc; }
        public ProjMessageModel(string desc, Int64 val) // this c-tor is for demo messages
        {
            _desc = desc;
            _int64value = val;
            _messageType = ProjMessageType.Counter;
        }
        public ProjMessageModel(string desc, string val) // this c-tor is for demo messages
        {
            _desc = desc;
            _stringValue = val;
            _messageType = ProjMessageType.Text;
        }
        public ProjMessageModel(byte[] receiveBuffer)
        {
            int projMessageLength = 232;
            int projMessageTextFieldLength = 200;

            // init conditions
            if ((null == receiveBuffer) || (receiveBuffer.Length !=projMessageLength))
            {
                AppServices.WriteToTrace("receive-buffer null or incorrect length");
                _messageType = ProjMessageType.Invalid;
                return;
            }
            if ((receiveBuffer[0] != 0xdf) || (receiveBuffer[1] != 0x45))
            {
                AppServices.WriteToTrace("receive-buffer incorrect preambles");
                _messageType = ProjMessageType.Invalid;
                return;
            }

            _messageType = (receiveBuffer[2] < 3) ? ((ProjMessageType)receiveBuffer[2]) : ProjMessageType.Invalid;
            if (_messageType == ProjMessageType.Invalid)
                return;

            // parse message
            try
            {
                _severity = BitConverter.ToInt32(receiveBuffer, 4);
                Int32 stringActualLength = BitConverter.ToInt32(receiveBuffer, 8);
                if (stringActualLength> projMessageTextFieldLength)
                {
                    stringActualLength = projMessageTextFieldLength;
                    AppServices.WriteToTrace("String length of message too long {0}", stringActualLength.ToString());
                }
                UInt32 secIntFrac = BitConverter.ToUInt32(receiveBuffer, 12);
                UInt64 sec = BitConverter.ToUInt64(receiveBuffer, 16);
                
                string theMessageText = System.Text.Encoding.ASCII.GetString(receiveBuffer, 32, stringActualLength);

                switch (MessageType)
                {
                    case ProjMessageType.Counter:
                        _stringValue = null;
                        _desc = theMessageText;
                        _int64value = BitConverter.ToInt64(receiveBuffer, 24);
                        break;
                    case ProjMessageType.Text:
                        {
                            string[] sa = theMessageText.Split('|');
                            if (sa.Length > 1)
                            {
                                _desc = sa[0];
                                _stringValue = sa[1];
                            }
                            else
                            {
                                _desc = theMessageText;
                                _stringValue = "<< no text in message >>";
                            }
                        }
                        break;
                    case ProjMessageType.SimpleLog:
                        _stringValue = theMessageText;
                        break;
                }

                // calc time
                double secFrac = (double)secIntFrac / fracFactor;
                _projTimeInSec = (double)sec + secFrac;
            }
            catch (Exception ex)
            {
                AppServices.WriteToTrace("Failed to parse message. " + ex.Message);
                _messageType = ProjMessageType.Invalid;
                return;
            }
        }

        public ProjMessageModel(JsonStatusClass js)
        {
            //this._jsonMessage = js;
            _messageType = ProjMessageType.Text;
            _desc = js.FieldTitle;
            _stringValue = js.FormattedStatus;
        }
    }
}
