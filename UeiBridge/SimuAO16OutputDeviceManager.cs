using System;
using System.Threading.Tasks;
using UeiDaq;
using UeiBridge.Types;
using System.Timers;
using UeiBridge.Library;

namespace UeiBridge
{
    public class AO16OutputDeviceManager : OutputDevice
    {
        #region === publics ====
        public override string DeviceName => DeviceMap2.AO308Literal;// "AO-308";
        public IWriterAdapter<double[]> AnalogWriter => _analogWriter;
        #endregion

        #region === privates ===
        IWriterAdapter<double[]> _analogWriter;
        log4net.ILog _logger = StaticMethods.GetLogger();
        System.Collections.Generic.List<ViewerItem<double>> _lastScanList;
        bool _inDisposeState = false;
        private IConvert2<double[]> _attachedConverter;
        #endregion

        SimuAO16Setup ThisDeviceSetup;

        public AO16OutputDeviceManager(SimuAO16Setup deviceSetup, IWriterAdapter<double[]> analogWriter) : base(deviceSetup)
        {
            _analogWriter = analogWriter;
            ThisDeviceSetup = deviceSetup as SimuAO16Setup;
            if (null == ThisDeviceSetup)
            {
                throw new ArgumentNullException();
            }
        }

        public AO16OutputDeviceManager() : base(null) { }

        public override bool OpenDevice()
        {
            try
            {
                _attachedConverter = new AnalogConverter(AI201100Setup.PeekVoltage_upstream, AO308Setup.PeekVoltage_downstream);

                int numOfCh = _analogWriter.OriginSession.GetNumberOfChannels();
                System.Diagnostics.Debug.Assert(numOfCh == 8);
                _lastScanList = new System.Collections.Generic.List<ViewerItem<double>>(new ViewerItem<double>[numOfCh]);

                Task.Factory.StartNew(() => OutputDeviceHandler_Task());

                var range = _analogWriter.OriginSession.GetDevice().GetAORanges();
                if (ThisDeviceSetup.IsBlockSensorActive)
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
            if (_inDisposeState)
            {
                return;
            }
            // write to device
            // ===============
            var p = _attachedConverter.DownstreamConvert(em.PayloadBytes);
            double[] scan = p as double[];
            System.Diagnostics.Debug.Assert(scan != null);

            _analogWriter.WriteSingleScan(scan);
            lock (_lastScanList)
            {
                for (int ch = 0; ch < scan.Length; ch++)
                {
                    _lastScanList[ch] = new ViewerItem<double>(scan[ch], timeToLiveMs: 5000);
                }
            }
        }

        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            // tbd: must lock. collection modified outside ......
            System.Text.StringBuilder formattedString = new System.Text.StringBuilder("Output voltage: ");
            lock (_lastScanList)
            {
                //if (_lastScanList[0]?.timeToLive.Ticks > 0)

                //_lastScanList[0].timeToLive -= interval;
                //if (null != _lastScanList)

                for (int entryIndex = 0; entryIndex < _lastScanList.Count; entryIndex++)
                {
                    formattedString.Append("  ");
                    if (_lastScanList[entryIndex] == null)
                    {
                        formattedString.Append("-.-");
                    }
                    else
                    {
                        if (_lastScanList[entryIndex].timeToLive.Ticks > 0)
                        {
                            _lastScanList[entryIndex].timeToLive -= interval;
                            formattedString.Append(_lastScanList[entryIndex].readValue.ToString("0.0"));
                        }
                        else
                        {
                            _lastScanList[entryIndex] = null;
                        }
                    }
                }


                //else
                //{
                //    formattedString.Append("- - -");
                //}
            }
            return new string[] { formattedString.ToString() };
        }

        public override void Dispose()
        {
            _inDisposeState = true;
            base.Dispose();

            if (null != _analogWriter.OriginSession)
            {
                if (_analogWriter.OriginSession.IsRunning())
                {
                    _analogWriter.OriginSession.Stop();
                }
                _analogWriter.OriginSession.Dispose();
            }
        }
    }

}