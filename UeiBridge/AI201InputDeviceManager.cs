﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;
using UeiBridge.Library.Types;
using UeiBridge.Library;
using UeiBridge.Library.CubeSetupTypes;
using UeiBridge.Library.Interfaces;

namespace UeiBridge
{
    /// <summary>
    /// ** from the manual:
    /// ** Sequential Sampling, 16-bit, 24-channel Analog Input board
    /// </summary>
    class AI201InputDeviceManager : InputDevice
    {
        public override string DeviceName => "AI-201-100"; // 

        private UeiDaq.AnalogScaledReader _reader;
        private readonly log4net.ILog _logger = StaticLocalMethods.GetLogger();
        private readonly IConvert2<double[]> _attachedConverter;
        private readonly AI201100Setup _thisSetup;
        private double[] _lastScan;
        private System.Threading.Timer _samplingTimer;

        const string _channelsString = "Ai0:23";
        //private ISession _ueiSession;
        //private ISend<SendObject> _targetConsumer;

        public AI201InputDeviceManager(AI201100Setup setup, ISession session,  ISend<SendObject> targetConsumer ) : base( setup)
        {
            _thisSetup = setup;
            _iSession = session;
            _targetConsumer = targetConsumer;

            _attachedConverter = new AnalogConverter(AI201100Setup.PeekVoltage_upstream, AO308Setup.PeekVoltage_downstream);
            System.Diagnostics.Debug.Assert(this.DeviceName.Equals(setup.DeviceName));
        }

        public AI201InputDeviceManager() { }// must have default const.

        public void HandleResponse_Callback(object state)
        {
            try
            {
                // read from device
                // ===============
                _lastScan = _reader.ReadSingleScan(); // access violation?
                System.Diagnostics.Debug.Assert(_lastScan != null);

                System.Diagnostics.Debug.Assert(_lastScan.Length == _iSession.GetNumberOfChannels(), "wrong number of channels");

                //ScanResult dr = new ScanResult(_lastScan, this);
                byte[] payload = _attachedConverter.UpstreamConvert(_lastScan);
                EthernetMessage em = StaticLocalMethods.BuildEthernetMessageFromDevice(payload, this._thisSetup);
                SendObject so = new SendObject(_thisSetup.DestEndPoint.ToIpEp(), em.GetByteArray( MessageWay.upstream));
                _targetConsumer.Send(so);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }

        public override bool OpenDevice()
        {
            double peek = AI201100Setup.PeekVoltage_upstream;
            try
            {
                string url1 = $"{_thisSetup.CubeUrl}Dev{_thisSetup.SlotNumber}/{_channelsString}";
                //_ueiSession = new Session();
                //_ueiSession.CreateAIChannel(url1, -peek, peek, AIChannelInputMode.SingleEnded); // -15,15 means 'no gain'
                var numberOfChannels = _iSession.GetNumberOfChannels();
                //_ueiSession.ConfigureTimingForSimpleIO();
                _reader = _iSession.GetAnalogScaledReader();
                    //new AnalogScaledReader(_ueiSession.GetDataStream());
                Range[] r = _iSession.GetDevice().GetAIRanges();
                TimeSpan interval = TimeSpan.FromMilliseconds(_thisSetup.SamplingInterval);
                _samplingTimer = new System.Threading.Timer(HandleResponse_Callback, null, TimeSpan.Zero, interval);
                EmitInitMessage($"Init success. {DeviceName}. {_iSession.GetNumberOfChannels()} input channels. Dest:{_thisSetup.DestEndPoint.ToIpEp()}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return false;
            }
        }
        public override void Dispose()
        {
            _samplingTimer.Dispose();
            _reader.Dispose();
            System.Threading.Thread.Sleep(200);
            _targetConsumer.Dispose();
            _iSession.Dispose();
            _logger.Debug($"{this.DeviceName}/Input, slot {_thisSetup.SlotNumber}, Disposed");
        }

        public override string []GetFormattedStatus( TimeSpan interval)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("Input voltage: ");
            if (null != _lastScan)
            {
                foreach (double d in _lastScan)
                {
                    sb.Append("  ");
                    sb.Append(d.ToString("0.0"));
                }
            }
            return new string[] { sb.ToString() };
        }
    }
}

