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
        public override string DeviceName => DeviceMap2.AO308Literal;// "AO-308";
        public IWriterAdapter<double[]> AnalogWriter => _analogWriter;

        public Session UeiSession { get => _session; }
        #endregion

        #region === privates ===
        IWriterAdapter<double[]> _analogWriter;
        log4net.ILog _logger = StaticMethods.GetLogger();
        List<ViewerItem<double>> _viewerItemList = new List<ViewerItem<double>>();
        //object _viewerItemListLock = new object();
        bool _inDisposeState = false;
        private IConvert2<double[]> _attachedConverter;
        UeiDaq.Session _session;
        #endregion

        public AO308OutputDeviceManager(DeviceSetup deviceSetup1, IWriterAdapter<double[]> analogWriter, UeiDaq.Session session) : base(deviceSetup1)
        {
            this._analogWriter = analogWriter;
            this._session = session;
        }

        public AO308OutputDeviceManager() : base(null) { }

        public override bool OpenDevice()
        {
            try
            {
                _attachedConverter = new AnalogConverter(AI201100Setup.PeekVoltage_upstream, AO308Setup.PeekVoltage_downstream);

                int numOfCh = _session.GetNumberOfChannels();
                System.Diagnostics.Debug.Assert(numOfCh == 8);

                Task.Factory.StartNew(() => OutputDeviceHandler_Task());

                var range = _session.GetDevice().GetAORanges();
                if ( _deviceSetup.IsBlockSensorActive) 
                {
                    _logger.Info($"Init success: {InstanceName} . { numOfCh} channels. Range {range[0].minimum},{range[0].maximum}V. Listening on BlockSensor");
                }
                else
                {
                    _logger.Info($"Init success: {InstanceName} . { numOfCh} channels. Range {range[0].minimum},{range[0].maximum}V. Listening on {_deviceSetup.LocalEndPoint.ToIpEp()}");
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
            if (em.NominalLength>32)
            {
                _logger.Warn($"Incoming message rejected. Payload length too large - {em.NominalLength}");
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
                _viewerItemList = ld.Select(t => new ViewerItem<double>(t, timeToLiveMs: 5000)).ToList();
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
            base.Dispose();

            _analogWriter.Dispose();
            if (null != _session)
            {
                if (_session.IsRunning())
                {
                    _session.Stop();
                }
                _session.Dispose();
            }
        }
    }
}