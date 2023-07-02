using System;
using System.Threading.Tasks;
using UeiBridge.Types;
using System.Timers;
using UeiBridge.Library;
using System.Collections.Generic;
using System.Linq;
using UeiDaq;

namespace UeiBridge
{

    /// <summary>
    /// from the manual:
    /// ** 8-Channel, 16-bit, ±10V Analog Output Board **
    /// </summary>
    public class AO308OutputDeviceManager : OutputDevice
    {
        #region === publics ====
        public override string DeviceName => DeviceMap2.AO308Literal;
        public IWriterAdapter<double[]> AnalogWriter => _analogWriter;
        public Session UeiSession { get => _ueiSession; }
        public bool IsBlockSensorActive { get; private set; }
        #endregion

        protected IWriterAdapter<double[]> _analogWriter;
        protected log4net.ILog _logger = StaticMethods.GetLogger();
        protected List<ViewItem<double>> _viewerItemList = new List<ViewItem<double>>();
        protected bool _inDisposeState = false;
        protected AnalogConverter _attachedConverter;

        protected Session _ueiSession;
        private DeviceSetup _deviceSetup;

        public AO308OutputDeviceManager(DeviceSetup deviceSetup1, IWriterAdapter<double[]> analogWriter, UeiDaq.Session session, bool isBlockSensorActive) : base(deviceSetup1)
        {
            this._analogWriter = analogWriter;
            this._ueiSession = session;
            this.IsBlockSensorActive = isBlockSensorActive;
            this._deviceSetup = deviceSetup1;
        }
        public AO308OutputDeviceManager() {} // must have empty c-tor

        public override bool OpenDevice()
        {
            try
            {
                _attachedConverter = new AnalogConverter(AI201100Setup.PeekVoltage_upstream, AO308Setup.PeekVoltage_downstream);

                int numOfCh = _ueiSession.GetNumberOfChannels();
                System.Diagnostics.Debug.Assert(numOfCh == 8);

                Task.Factory.StartNew(() => OutputDeviceHandler_Task());

                var range = _ueiSession.GetDevice().GetAORanges();
                AO308Setup ao308 = _deviceSetup as AO308Setup;
                //System.Diagnostics.Debug.Assert(this.IsBlockSensorActive.HasValue);
                if ( this.IsBlockSensorActive) 
                {
                    EmitInitMessage($"Init success: {DeviceName} . { numOfCh} channels. Range {range[0].minimum},{range[0].maximum}V. Listening on BlockSensor");
                }
                else
                {
                    EmitInitMessage($"Init success: {DeviceName} . { numOfCh} channels. Range {range[0].minimum},{range[0].maximum}V. Listening on {_deviceSetup.LocalEndPoint.ToIpEp()}");
                }

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
            lock (_viewerItemList)
            {
                _viewerItemList = ld.Select(t => new ViewItem<double>(t, timeToLiveMs: 5000)).ToList();
            }
        }

        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            System.Text.StringBuilder formattedString = new System.Text.StringBuilder("Output voltage: ");

            lock (_viewerItemList)
            {
                foreach(var item in _viewerItemList)
                {
                    formattedString.Append("  ");
                    if (item == null)
                    {
                        formattedString.Append("-.-");
                    }
                    else
                    {
                        if (item.timeToLive.Ticks > 0)
                        {
                            item.timeToLive -= interval;
                            formattedString.Append( item.readValue.ToString("0.0"));
                        }
                    }
                }
            }
            return new string[] { formattedString.ToString() };
        }

        public override void Dispose()
        {
            _inDisposeState = true;
            //base.HaltMessageLoop();
            _analogWriter.Dispose();
            CloseSession(_ueiSession);
            base.Dispose();
        }
    }
}