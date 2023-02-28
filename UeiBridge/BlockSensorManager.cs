using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.Types;
using UeiBridge.Library;

namespace UeiBridge
{
    /// <summary>
    /// This manager handles the 'block sensor'.
    /// It gets a udp message which define series of voltage values.
    /// According to input from digital card, it decide into which analog output is should emit this values.
    /// </summary>
    class BlockSensorManager : OutputDevice, ISend<SendObject>
    {
        #region === publics ====
        public override string DeviceName => "BlockSensor";
        public override string InstanceName => "BlockSensorManager";
        #endregion
        #region === privates ===
        //AO308OutputDeviceManager _ao308Device;
        log4net.ILog _logger = StaticMethods.GetLogger();
        List<BlockSensorEntry> _blockSensorTable = new List<BlockSensorEntry>();
        DIO403Convert _digitalConverter = new DIO403Convert( null);
        IAnalogWrite _analogWriter;
        int _subaddress;
        double[] _analogScan;
        BlockSensorSetup _thisDeviceSetup;
        #endregion

        public BlockSensorManager(DeviceSetup deviceSetup, IAnalogWrite writer) : base(deviceSetup)
        {
            System.Diagnostics.Debug.Assert(writer != null);
            _thisDeviceSetup = deviceSetup as BlockSensorSetup;
            System.Diagnostics.Debug.Assert(null != _thisDeviceSetup);
            _analogWriter = writer;

            _blockSensorTable.Add(new BlockSensorEntry("ps1", 4, 0));
            _blockSensorTable.Add(new BlockSensorEntry("ps2", 6, 1));
            _blockSensorTable.Add(new BlockSensorEntry("ps3", 5, 2));
            _blockSensorTable.Add(new BlockSensorEntry("pd1", 1, 0));
            _blockSensorTable.Add(new BlockSensorEntry("pd2", 1, 1));
            _blockSensorTable.Add(new BlockSensorEntry("pd3", 1, 2));
            _blockSensorTable.Add(new BlockSensorEntry("t1",  2, 0));
            _blockSensorTable.Add(new BlockSensorEntry("t2",  2, 1));
            _blockSensorTable.Add(new BlockSensorEntry("t3",  4, 2));
            _blockSensorTable.Add(new BlockSensorEntry("vref1", 5, 0));
            _blockSensorTable.Add(new BlockSensorEntry("vref2", 5, 1));
            _blockSensorTable.Add(new BlockSensorEntry("vref3", 7, 2));
            _blockSensorTable.Add(new BlockSensorEntry("vref4", 0, 3));
            _blockSensorTable.Add(new BlockSensorEntry("p5v3", 1, 3));

            _analogScan = new double[writer.NumberOfChannels];
            Array.Clear(_analogScan, 0, _analogScan.Length);
        }
        public BlockSensorManager() : base(null) // empty c-tor for Activator.CreateInstance()
        {
        }

        public void Start()
        {

        }
        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            return new string[] { "block sensor not ready yet" };
        }

        public override bool OpenDevice()
        {
            _logger.Info($"Init success: {InstanceName} . Listening on { _thisDeviceSetup.LocalEndPoint.ToIpEp()}");
            // device shall be opened upon first setup message (from simulator)
            return true;
        }

        protected override void HandleRequest(EthernetMessage request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Two types of messages might reach here
        /// 1. From "DIO-403/Input". (a copy of the upstream message)
        /// 2. From Ethernet. id of this message is 32 ("to BlockSensor").
        /// </summary>
        public override void Enqueue(byte[] byteMessage)
        {
            // upstream message from digital/input card
            if (byteMessage[EthernetMessage._cardTypeOffset] == StaticMethods.GetCardIdFromCardName("DIO-403"))
            {
                // convert
                try
                {
                    EthernetMessage msg = EthernetMessage.CreateFromByteArray(byteMessage, MessageDirection.upstream);
                    System.Diagnostics.Debug.Assert(null != msg);
                    //byte[] digitalVector = _digitalConverter.EthToDevice(msg.PayloadBytes) as byte[];
                    //System.Diagnostics.Debug.Assert(null != digitalVector);

                    _subaddress = msg.PayloadBytes[0] & 0x7; // get lower 3 bits
                }
                catch (ArgumentException ex)
                {
                    _logger.Warn($"BlockSensor: {ex.Message}");
                }

                return;
            }

            // downstream message aimed to block sensor
            if (byteMessage[EthernetMessage._cardTypeOffset] == StaticMethods.GetCardIdFromCardName("BlockSensor"))
            {
                var selectedEntries = _blockSensorTable.Where(ent => ent.Subaddress == this._subaddress);
                foreach (var entry in selectedEntries)
                {
                    int line = entry.chan_ain;
                    EthernetMessage msg = EthernetMessage.CreateFromByteArray(byteMessage, MessageDirection.downstream);
                    DeviceSetup ds = Config2.Instance.GetSetupEntryForDevice(0, "AO-308");
                    var converter = (IConvert)Activator.CreateInstance(typeof( AO308Convert), ds);
                    double [] scan = converter.EthToDevice(msg.PayloadBytes) as double[];
                    System.Diagnostics.Debug.Assert(scan != null);
                    _analogScan[line] = scan[line];
                    // emit to analog card
                    _analogWriter.WriteSingleScan(_analogScan);
                }
            }
        }

        public void Send(SendObject obj)
        {
            this.Enqueue(obj.ByteMessage);
        }
        //internal void SetAnalogOuputInterface(AO308OutputDeviceManager ao308)
        //{
        //    _ao308Device = ao308;
        //}
    }
    class BlockSensorEntry
    {
        public string SignalName { get; private set; }
        public int chan_ain { get; private set; }
        public int Subaddress { get; private set; }

        public BlockSensorEntry(string signalName, int chan_ain, int subaddress)
        {
            SignalName = signalName;
            this.chan_ain = chan_ain;
            Subaddress = subaddress;
        }
    }
}
