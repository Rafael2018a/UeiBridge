﻿using System;
using System.Collections.Generic;
using System.Text;
using UeiDaq;
using UeiBridge.Types;
using UeiBridge.Library;
using System.Threading;
using System.Linq;

namespace UeiBridge
{

    /// <summary>
    /// "SL-508-892" manager.
    /// R&R: Reads from serial device and sends the result to 'targetConsumer'
    /// This class is responsible for disposing the serial session.
    /// </summary>
    class SL508InputDeviceManager : InputDevice
    {
        public override string DeviceName => "SL-508-892";

        private log4net.ILog _logger = StaticMethods.GetLogger();
        private readonly List<SerialReader> _serialReaderList = new List<SerialReader>();
        private bool _InDisposeState = false;
        private List<ViewItem<byte[]>> _lastScanList = new List<ViewItem<byte[]>>();
        private readonly SL508892Setup _thisDeviceSetup;
        private ISend<SendObject> _targetConsumer;
        private SessionEx _serialSession;
        private List<IAsyncResult> _readerIAsyncResultList;
        private const int minLen = 200;
        

        public SL508InputDeviceManager(ISend<SendObject> targetConsumer, DeviceSetup setup, SessionEx serialSession) : base( setup)
        {
            _targetConsumer = targetConsumer;
            _thisDeviceSetup = setup as SL508892Setup;
            _serialSession = serialSession;

            System.Diagnostics.Debug.Assert(null != _targetConsumer);
            System.Diagnostics.Debug.Assert(null != _thisDeviceSetup);
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
                    if (item.timeToLive.Ticks > 0)
                    {
                        item.timeToLive -= interval;
                        int len = (item.readValue.Length > 20) ? 20 : item.readValue.Length;
                        string s = $"Ch{ch}: Payload=({item.readValue.Length}): {BitConverter.ToString(item.readValue).Substring(0, len * 3 - 1)}";
                        resultList.Add(s);
                        //formattedString.Append(s);
                    }
                    else
                    {
                        resultList.Add( $"Ch{ch}: <empty>");
                    }
                }
                else
                {
                    resultList.Add($"Ch{ch}: <empty>");
                }
            }
            return resultList.ToArray();
        }

        public void ReaderCallback(IAsyncResult ar)
        {
            int channel = (int)ar.AsyncState;
            try
            {
                byte[] receiveBuffer = _serialReaderList[channel].EndRead(ar);
                // this api might throw UeiDaqException exception with message "The device is not responding, check the connection and the device's status"
                // in this case, the session must be closed/disposed and open again.

                // ex.Message = "An error occurred while accessing the device"

                _lastScanList[channel] = new ViewItem<byte[]>(receiveBuffer, timeToLiveMs: 5000);
                byte [] payload = receiveBuffer;
                EthernetMessage em = StaticMethods.BuildEthernetMessageFromDevice(payload, this._thisDeviceSetup, channel);
                // forward to consumer (send by udp)
                _targetConsumer.Send(new SendObject( _thisDeviceSetup.DestEndPoint.ToIpEp(), em.GetByteArray( MessageWay.upstream)));

                // restart reader
                _readerIAsyncResultList[channel] = _serialReaderList[channel].BeginRead(minLen, this.ReaderCallback, channel);
            }
            catch (UeiDaqException ex)
            {
                if (Error.Timeout == ex.Error)
                {
                    // Ignore timeout error, they will occur if the send button is not
                    // clicked on fast enough!
                    if (_InDisposeState == false)
                    {
                        _readerIAsyncResultList[channel] = _serialReaderList[channel].BeginRead(minLen, this.ReaderCallback, channel);
                    }
                    else
                    {
                        _logger.Debug($"Disposing {InstanceName} ch{channel}");
                        // tbd. Dispose session here?
                    }
                }
                else
                {
                    _logger.Warn($"ReaderCallback:  {InstanceName}. {ex.Message}.");

                    // tbd. Dispose session here?
                }
            }
            catch(Exception ex)
            {
                _logger.Warn($"ReaderCallback: {InstanceName}. {ex.Message}");
            }
        }
        public override bool OpenDevice()
        {
            _lastScanList = new List<ViewItem<byte[]>>(new ViewItem<byte[]>[_serialSession.GetNumberOfChannels()]);
            _readerIAsyncResultList = new List<IAsyncResult>(new IAsyncResult[_thisDeviceSetup.Channels.Count]);

            for (int ch = 0; ch < _serialSession.GetNumberOfChannels(); ch++)
            {
                var sr = new SerialReader(_serialSession.GetDataStream(), _serialSession.GetChannel(ch).GetIndex());
                _serialReaderList.Add(sr);
            }
            System.Threading.Thread.Sleep(10);

            // activate
            int ch1 = 0;
            foreach (var sl in _serialReaderList)
            {
                AsyncCallback readerAsyncCallback = new AsyncCallback(ReaderCallback);
                _readerIAsyncResultList[ch1] = sl.BeginRead(minLen, readerAsyncCallback, ch1);
                ch1++;
            }

            EmitInitMessage( $"Init success {DeviceName}. {_serialSession.GetNumberOfChannels()} channels. Dest:{ _thisDeviceSetup.DestEndPoint.ToIpEp()}");

            return true;
        }
        public override void Dispose()
        {
            _InDisposeState = true;

            var waitall = _readerIAsyncResultList.Select(i => i.AsyncWaitHandle).ToArray();
            WaitHandle.WaitAll(waitall);

            //_logger.Debug($"Disposing {this.DeviceName}/Input, slot {_thisDeviceSetup.SlotNumber}");
            //if (_serialSession.IsRunning())
            try
            {
                _serialSession.Stop();
            }
            catch (UeiDaq.UeiDaqException ex)
            {
                _logger.Debug($"Session stop() failed. {ex.Message}");
            }
            _serialSession.Dispose();
        }
    }
}
