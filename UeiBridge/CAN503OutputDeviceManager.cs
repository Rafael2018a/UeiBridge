using System;
using System.Threading.Tasks;
using UeiDaq;
using UeiBridge.Library;
using System.Collections.Generic;
using UeiBridge.Library.CubeSetupTypes;

namespace UeiBridge
{
    class CAN503OutputDeviceManager : OutputDevice
    {
        public override string DeviceName => DeviceMap2.CAN503Literal;

        private SessionAdapter _canSession;
        log4net.ILog _logger = StaticLocalMethods.GetLogger();
        private CAN503Setup _thisSetup;
        List<CANWriterAdapter> _canWriterList = new List<CANWriterAdapter>();
        
        public CAN503OutputDeviceManager()
        {
        }

        public CAN503OutputDeviceManager(DeviceSetup deviceSetup, SessionAdapter canSession) : base(deviceSetup)
        {
            this._canSession = canSession;
            this._thisSetup = deviceSetup as CAN503Setup;
        }

        public override void Dispose()
        {
            _inDisposeFlag = true;
            base.TerminateMessageLoop();
            for (int ch = 0; ch < _canWriterList.Count; ch++)
            {
                _canWriterList[ch].Dispose();
            }
            //_canSession.Dispose();
            
        }

        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            return null;
        }

        public override bool OpenDevice()
        {
            EmitInitMessage($"Init success {DeviceName}. Listening on {_thisSetup.LocalEndPoint.ToIpEp()}");
            Task.Factory.StartNew(() => OutputDeviceHandler_Task());
            _isDeviceReady = true;

            // verify init conditions
            if ((_canSession == null) || (_thisSetup == null))
            {
                _logger.Warn($"Failed to open device {this.InstanceName}");
                return false;
            }

            // build writer list
            for (int ch = 0; ch < _canSession.GetNumberOfChannels(); ch++)
            {
                System.Threading.Thread.Sleep(10);
                CANWriterAdapter cra = _canSession.GetCANWriter(ch);
                _canWriterList.Add(cra);
            }

            EmitInitMessage($"Init success {DeviceName}. Listening on {_thisSetup.LocalEndPoint.ToIpEp()}");

            _isDeviceReady = true;

            return true;
        }

        protected override void HandleRequest(EthernetMessage request)
        {
            if (true == _inDisposeFlag)
            {
                return;
            }

            CANFrame[] frames = new CANFrame[1];
            frames[0] = new CANFrame();

            frames[0].Id = BitConverter.ToUInt32(request.PayloadBytes, 0);
            byte t = request.PayloadBytes[4];
            frames[0].Type = (1 == t) ? CANFrameType.RemoteFrame : CANFrameType.DataFrame;
            frames[0].DataSize = Convert.ToUInt32( request.PayloadBytes.Length - 5);
            byte[] canData = new byte[frames[0].DataSize];
            //request.PayloadBytes.CopyTo((canData, 5);
            Array.Copy(request.PayloadBytes, 5, canData, 0, canData.Length);
            frames[0].Data = canData;
            System.Diagnostics.Debug.Assert(request.SerialChannelNumber < 5);
            _canWriterList[request.SerialChannelNumber].Write(frames);

        }
    }


}

