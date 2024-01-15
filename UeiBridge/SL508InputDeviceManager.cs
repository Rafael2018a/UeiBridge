using System;
using System.Collections.Generic;
using System.Text;
using UeiDaq;
using UeiBridge.Types;
using UeiBridge.Library;
using System.Threading;
using System.Linq;
using UeiBridge.CubeSetupTypes;
using UeiBridge.Interfaces;

namespace UeiBridge
{

    class ChannelAux
    {
        public ChannelAux(int channel)
        {
            ChannelNumber = channel;
        }
        public SerialReader Reader { get; set; }
        public IAsyncResult AsyncResult { get; set; }
        public AsyncCallback Callback { get; set; }
        public int ChannelNumber { get;  private set; }
    }

    /// <summary>
    /// "SL-508-892" manager.
    /// R&R: Reads from serial device and sends the result to 'targetConsumer'
    /// This class is responsible for disposing the serial session.
    /// </summary>
    class SL508InputDeviceManager : InputDevice
    {
        public override string DeviceName => DeviceMap2.SL508Literal; //"SL-508-892";

        private log4net.ILog _logger = StaticMethods.GetLogger();
        //private readonly List<SerialReader> _serialReaderList = new List<SerialReader>();
        private bool _InDisposeState = false;
        private List<ViewItem<byte[]>> _lastScanList = new List<ViewItem<byte[]>>();
        private readonly SL508892Setup _thisSetup;
        //private ISend<SendObject> _targetConsumer;
        private SessionAdapter _serialSession;
        //private List<IAsyncResult> _readerIAsyncResultList;
        //private List<AsyncCallback> _readerCallbackList;
        private const int minLen = 200;
        List<ChannelAux> _channelAuxList;
        

        public SL508InputDeviceManager(ISend<SendObject> targetConsumer, DeviceSetup setup, SessionAdapter serialSession) : base( setup)
        {
            _targetConsumer = targetConsumer;
            _thisSetup = setup as SL508892Setup;
            _serialSession = serialSession;

            System.Diagnostics.Debug.Assert(null != _targetConsumer);
            System.Diagnostics.Debug.Assert(null != _thisSetup);
            System.Diagnostics.Debug.Assert(null != serialSession);
            System.Diagnostics.Debug.Assert(this.DeviceName.Equals(setup.DeviceName));
        }

        public SL508InputDeviceManager() {}

        public override string [] GetFormattedStatus( TimeSpan interval)
        {
            List<string> resultList = new List<string>();
            for (int ch = 0; ch < _lastScanList.Count; ch++)
            {
                var item = _lastScanList[ch];
                if (null != item)
                {
                    if (item.TimeToLive > TimeSpan.Zero)
                    {
                        item.DecreaseTimeToLive( interval);
                        int len = (item.ReadValue.Length > 20) ? 20 : item.ReadValue.Length;
                        string s = $"Ch{ch}: Payload=({item.ReadValue.Length}): {BitConverter.ToString(item.ReadValue).Substring(0, len * 3 - 1)}";
                        resultList.Add(s);
                    }
                    else
                    {
                        item = null;
                    }
                    //else
                    //{
                    //    resultList.Add( $"Ch{ch}: <empty>");
                    //}
                }
                //else
                //{
                //    resultList.Add($"Ch{ch}: <empty>");
                //}
            }
            if (resultList.Count > 0)
            {
                return resultList.ToArray();
            }
            else
            {
                return null;
            }
        }

        public void ReaderCallback(IAsyncResult ar)
        {
            int channel = (int)ar.AsyncState;
            ChannelAux chAux = _channelAuxList[channel];
            System.Diagnostics.Debug.Assert(channel == chAux.ChannelNumber);
            try
            {
                byte[] receiveBuffer = chAux.Reader.EndRead(ar); 

                // update status
                _lastScanList[channel] = new ViewItem<byte[]>(receiveBuffer, TimeSpan.FromSeconds(5));
                EthernetMessage em = StaticMethods.BuildEthernetMessageFromDevice(receiveBuffer, this._thisSetup, channel);
                // send to consumer
                _targetConsumer.Send(new SendObject( _thisSetup.DestEndPoint.ToIpEp(), em.GetByteArray( MessageWay.upstream)));

                // restart reader
                if (_InDisposeState == false)
                {
                    System.Diagnostics.Debug.Assert(true == _serialSession.IsRunning());
                    chAux.AsyncResult = chAux.Reader.BeginRead(minLen, chAux.Callback, channel);
                }
            }
            catch (UeiDaqException ex)
            {
                if (Error.Timeout == ex.Error)
                {
                    if (_InDisposeState == false)
                    {
                        System.Diagnostics.Debug.Assert(true == _serialSession.IsRunning());
                        chAux.AsyncResult = chAux.Reader.BeginRead(minLen, chAux.Callback, channel);
                    }
                    else
                    {
                        _logger.Debug($"Terminating {InstanceName} ch{channel}");
                    }
                }
                else
                {
                    _logger.Warn($"{InstanceName} ch{channel}. Serial input error: {ex.Message}.");
                    Thread.Sleep(500);
                    chAux.AsyncResult = chAux.Reader.BeginRead(minLen, chAux.Callback, channel);
                }
            }
            catch(Exception ex)
            {
                _logger.Warn($"{InstanceName} ch{channel}. Serial input global error: {ex.Message}.");
            }
        }
        public override bool OpenDevice()
        {
            int numberOfChannels = _serialSession.GetNumberOfChannels();
            _lastScanList = new List<ViewItem<byte[]>>(new ViewItem<byte[]>[numberOfChannels]);
            _channelAuxList = new List<ChannelAux>();

            // build channelAux list 
            for (int ch = 0; ch < numberOfChannels; ch++)
            {
                ChannelAux newaux = new ChannelAux(ch);
                _channelAuxList.Add(newaux);
                newaux.Reader = _serialSession.GetSerialReader(ch);
                newaux.Callback = new AsyncCallback(ReaderCallback);
                newaux.AsyncResult = newaux.Reader.BeginRead(minLen, newaux.Callback, ch);
            }

            EmitInitMessage( $"Init success {DeviceName}. {_serialSession.GetNumberOfChannels()} channels. Dest:{ _thisSetup.DestEndPoint.ToIpEp()}");

            return true;
        }
        public override void Dispose()
        {
            _InDisposeState = true;
            _serialSession.Stop();
            var waitall = _channelAuxList.Select(i => i.AsyncResult.AsyncWaitHandle).ToArray();
            WaitHandle.WaitAll(waitall);
            foreach( ChannelAux rc in _channelAuxList)
            {
                rc.Reader.Dispose();
            }
            //for (int ch = 0; ch < _serialReaderList.Count; ch++)
            //{
            //    _serialReaderList[ch].Dispose();
            //}
            _serialSession.Dispose();
            _targetConsumer.Dispose();

            _logger.Debug($"{this.DeviceName}/Input, slot {_thisSetup.SlotNumber}, Disposed");
        }
    }
}
