using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.CubeSetupTypes;
using UeiBridge.Interfaces;
using UeiBridge.Library;
using UeiBridge.Types;
//#if dont
namespace UeiBridge.DevManagers
{
    /// <summary>
    /// Root class for analog output devices
    /// </summary>
    public abstract class AnalogOutDeviceManager: OutputDevice
    {
        #region === publics ====
        //public override string DeviceName => DeviceMap2.AO308Literal;
        //public IWriterAdapter<double[]> AnalogWriter => _analogWriter;
        public ISession SessionAdapter { get => _ueiSession; }
        public bool IsBlockSensorActive { get; private set; }
        #endregion
        
        protected IWriterAdapter<double[]> _analogWriter;
        private log4net.ILog _logger = StaticMethods.GetLogger();
        protected ViewItem<List<double>> _viewerItemList2;
        protected object _viewerItemListLockObject = new object();

        protected AnalogConverter _attachedConverter;

        private AnalogOutDeviceSetup _deviceSetup;

        public AnalogOutDeviceManager( AnalogOutDeviceSetup deviceSetup1, ISession session, bool isBlockSensorActive) : base(deviceSetup1)
        {
            //this._analogWriter = analogWriter;
            this._ueiSession = session;
            this.IsBlockSensorActive = isBlockSensorActive;
            this._deviceSetup = deviceSetup1;
        }
        //public AO308OutputDeviceManager() { } // must have empty c-tor
        public AnalogOutDeviceManager() { }
        public override bool OpenDevice()
        {
            try
            {
                _analogWriter = _ueiSession.GetAnalogScaledWriter();
                _attachedConverter = new AnalogConverter(AI201100Setup.PeekVoltage_upstream, AO308Setup.PeekVoltage_downstream);

                int numOfCh = _ueiSession.GetNumberOfChannels();
                System.Diagnostics.Debug.Assert(numOfCh > 0);

                Task.Factory.StartNew(() => OutputDeviceHandler_Task());

                UeiDaq.Range[] range = _ueiSession.GetDevice().GetAORanges();
                AO308Setup ao308 = _deviceSetup as AO308Setup;
                if (this.IsBlockSensorActive)
                {
                    EmitInitMessage($"Init success: {DeviceName} . {numOfCh} channels. Range {range[0].minimum},{range[0].maximum}V. Listening on BlockSensor");
                }
                else
                {
                    int deviceId = DeviceMap2.GetDeviceName(DeviceName);
                    EmitInitMessage($"Init success: {DeviceName} (ID={deviceId}). {numOfCh} channels. Range {range[0].minimum},{range[0].maximum}V. Listening on {_deviceSetup.LocalEndPoint.ToIpEp()}");
                }

                _viewerItemList2 = new ViewItem<List<double>>(new List<double>(new double[numOfCh]), TimeSpan.FromSeconds(5));

                _isDeviceReady = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return false;
            }
            return true;
        }

        protected override void HandleRequest(EthernetMessage em)
        {
            // init conditions check
            if (_inDisposeState)
            {
                return;
            }

            // write to device
            // ===============
            double[] outscan = _attachedConverter.DownstreamConvert(em.PayloadBytes);
            System.Diagnostics.Debug.Assert(outscan != null);
            _analogWriter.WriteSingleScan(outscan);

            // update for GetFormattedStatus()
            List<double> ld = new List<double>(outscan);
            //lock (_viewerItemList)
            //{
            //    _viewerItemList = ld.Select(t => new ViewItem<double>(t, TimeSpan.FromSeconds(5))).ToList();
            //}
            lock (_viewerItemListLockObject)
            {
                _viewerItemList2 = new ViewItem<List<double>>(ld, TimeSpan.FromMilliseconds(5000));
            }
        }

        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            System.Text.StringBuilder formattedString = null;

            if ((null != _viewerItemList2) && (_viewerItemList2.TimeToLive > TimeSpan.Zero))
            {
                formattedString = new System.Text.StringBuilder("Output voltage: ");
                lock (_viewerItemListLockObject)
                {
                    _viewerItemList2.DecreaseTimeToLive(interval);
                    foreach (double item in _viewerItemList2.ReadValue)
                    {
                        formattedString.Append("  ");
                        formattedString.Append(item.ToString("0.0"));

                    }
                }
                return new string[] { formattedString.ToString() };
            }
            else
            {
                return null;
            }
        }

        public override void Dispose()
        {
            _inDisposeState = true;
            //base.HaltMessageLoop();
            _analogWriter.Dispose();
            _ueiSession.Dispose();
            base.TerminateMessageLoop();
        }

    }
}
//#endif