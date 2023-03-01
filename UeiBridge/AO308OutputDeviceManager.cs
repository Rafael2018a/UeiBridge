using System;
using System.Threading.Tasks;
using UeiDaq;
using UeiBridge.Types;
using System.Timers;
using UeiBridge.Library;

namespace UeiBridge
{
    public class AnalogWriteAdapter: IAnalogWriter
    {
        AnalogScaledWriter _ueiAnalogWriter;
        //string _deviceUrl;

        //public AnalogWriteAdapter(string deviceUrl)
        //{
        //    _deviceUrl = deviceUrl;
        //}

        public AnalogWriteAdapter(AnalogScaledWriter analogWriter, int numberOfChannels)
        {
            this._ueiAnalogWriter = analogWriter;
            this.NumberOfChannels = numberOfChannels;
        }

        public int NumberOfChannels { get; set; }

        //public bool WriteScan(double[] scen)
        //{
        //    throw new NotImplementedException();
        //}

        public void WriteSingleScan( double [] scan)
        {
            _ueiAnalogWriter.WriteSingleScan(scan);
        }
    }
    /// <summary>
    /// from the manual:
    /// ** 8-Channel, 16-bit, ±10V Analog Output Board **
    /// </summary>
    internal class AO308OutputDeviceManager : OutputDevice
    {
        // publics
        public override string DeviceName => "AO-308";
        public override string InstanceName { get; }
        public IAnalogWriter AnalogWriter => _writer;
        // privates
        //AnalogScaledWriter _writer;
        AnalogWriteAdapter _writer;
        log4net.ILog _logger = StaticMethods.GetLogger();
        const string _channelsString = "Ao0:7";
        Session _deviceSession;
        System.Collections.Generic.List<ViewerItem<double>> _lastScanList;
        AO308Setup _thisDeviceSetup;
        bool _inDisposeState = false;

        private IConvert _attachedConverter;
        public AO308OutputDeviceManager(AO308Setup deviceSetup) : base(deviceSetup)
        {
            InstanceName = $"{DeviceName}/Slot{deviceSetup.SlotNumber}/Output";
            _thisDeviceSetup = deviceSetup;
        }

        public AO308OutputDeviceManager() : base(null)
        {
        }

        public override bool OpenDevice()
        {
            try
            {
                _attachedConverter = StaticMethods.CreateConverterInstance(_deviceSetup);

                string cubeUrl = $"{_deviceSetup.CubeUrl}Dev{_deviceSetup.SlotNumber}/{_channelsString}";

                _deviceSession = new Session();
                var c = _deviceSession.CreateAOChannel(cubeUrl, -AO308Setup.PeekVoltage_downstream, AO308Setup.PeekVoltage_downstream);
                System.Diagnostics.Debug.Assert(c.GetMaximum() == AO308Setup.PeekVoltage_downstream);
                _deviceSession.ConfigureTimingForSimpleIO();
                _writer = new AnalogWriteAdapter( new AnalogScaledWriter(_deviceSession.GetDataStream()), _deviceSession.GetNumberOfChannels());

                _lastScanList = new System.Collections.Generic.List<ViewerItem<double>>(new ViewerItem<double>[_deviceSession.GetNumberOfChannels()]);

                Task.Factory.StartNew(() => OutputDeviceHandler_Task());

                var range = _deviceSession.GetDevice().GetAORanges();
                _logger.Info($"Init success: {InstanceName} . { _deviceSession.GetNumberOfChannels()} channels. Range {range[0].minimum},{range[0].maximum}V. Listening on {_deviceSetup.LocalEndPoint.ToIpEp()}");

                _isDeviceReady = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _deviceSession = null;
                return false;
            }
            return true;
        }

        protected override void HandleRequest(EthernetMessage em)
        {
            if (_inDisposeState)
            {
                return;
            }
            // write to device
            // ===============
            var p = _attachedConverter.EthToDevice(em.PayloadBytes);
            double [] scan = p as double[];
            System.Diagnostics.Debug.Assert(scan != null);
            
            _writer.WriteSingleScan( scan);
            lock (_lastScanList)
            {
                for (int ch = 0; ch < scan.Length; ch++)
                {
                    _lastScanList[ch] = new ViewerItem<double>(scan[ch], timeToLiveMs: 5000);
                }
            }
        }
        
        public override string [] GetFormattedStatus( TimeSpan interval)
        {
            // tbd: must lock. collection modified outside ......
            System.Text.StringBuilder formattedString = new System.Text.StringBuilder("Output voltage: ");
            lock (_lastScanList)
            {
                if (_lastScanList[0]?.timeToLive.Ticks > 0)
                {
                    _lastScanList[0].timeToLive -= interval;
                    if (null != _lastScanList)
                    {
                        foreach (var vi in _lastScanList)
                        {
                            formattedString.Append("  ");
                            formattedString.Append(vi.readValue.ToString("0.0"));
                        }
                    }
                }

                else
                {
                    formattedString.Append("- - -");
                }
            }
            return new string[] { formattedString.ToString() };
        }

        public override void Dispose()
        {
            _inDisposeState = true;
            base.Dispose();

            //if (null != _writer)
            //{
            //    _writer.Dispose();
            //}
            if (null != _deviceSession)
            {
                _deviceSession.Stop();
                _deviceSession.Dispose();
            }
        }
        public virtual void Dispose1()
        {
            _deviceSession = null;
        }

        //protected override void resetLastScanTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    if (0 == e.SignalTime.Second % 10)
        //    {
        //        //_lastScanList = null;
        //    }
        //}
    }

    internal class SimuAO16 : OutputDevice
    {
        public override string DeviceName => "Simu-AO16";
        public override string InstanceName { get; }
        log4net.ILog _logger = StaticMethods.GetLogger();

        public SimuAO16(DeviceSetup deviceSetup) : base(deviceSetup as AO308Setup)
        {
            InstanceName = $"{DeviceName}/Slot{deviceSetup.SlotNumber}/Output";
        }
        public SimuAO16() : base(null) { }

        public override bool OpenDevice()
        {
            _logger.Info($"Init success: {InstanceName}");
            return true;
        }

        protected override void HandleRequest(EthernetMessage request)
        {
            throw new NotImplementedException();
        }

        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            return new string[] { "Simu-Ao16" };
        }
    }
}