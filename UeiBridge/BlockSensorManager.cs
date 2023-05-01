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
    public class BlockSensorManager : OutputDevice, ISend<SendObject>
    {
        #region === publics ====
        public override string DeviceName => DeviceMap2.BlocksensorLiteral; //"BlockSensor";
        #endregion
        #region === privates ===
        log4net.ILog _logger = StaticMethods.GetLogger();
        List<BlockSensorEntry> _blockSensorTable = new List<BlockSensorEntry>();
        IWriterAdapter<double[]> _analogWriter;
        int _subaddress = -1;
        double[] _scanToEmit;
        BlockSensorSetup ThisDeviceSetup => _deviceSetup as BlockSensorSetup;

        bool _isInDispose = false;
        #endregion

        public BlockSensorManager(DeviceSetup deviceSetup, IWriterAdapter<double[]> writer, UeiDaq.Session session) : base(deviceSetup)
        {
            System.Diagnostics.Debug.Assert(writer != null);
            System.Diagnostics.Debug.Assert(null != ThisDeviceSetup);
            _analogWriter = writer;

            int serial = 0;
            _blockSensorTable.Add(new BlockSensorEntry(serial++, "ps1", 4, 0));
            _blockSensorTable.Add(new BlockSensorEntry(serial++, "ps2", 6, 1));
            _blockSensorTable.Add(new BlockSensorEntry(serial++, "ps3", 5, 2));
            _blockSensorTable.Add(new BlockSensorEntry(serial++, "pd1", 1, 0));
            _blockSensorTable.Add(new BlockSensorEntry(serial++, "pd2", 1, 1));
            _blockSensorTable.Add(new BlockSensorEntry(serial++, "pd3", 1, 2));
            _blockSensorTable.Add(new BlockSensorEntry(serial++, "t1", 2, 0));
            _blockSensorTable.Add(new BlockSensorEntry(serial++, "t2", 2, 1));
            _blockSensorTable.Add(new BlockSensorEntry(serial++, "t3", 4, 2));
            _blockSensorTable.Add(new BlockSensorEntry(serial++, "vref1", 5, 0));
            _blockSensorTable.Add(new BlockSensorEntry(serial++, "vref2", 5, 1));
            _blockSensorTable.Add(new BlockSensorEntry(serial++, "vref3", 7, 2));
            _blockSensorTable.Add(new BlockSensorEntry(serial++, "vref4", 0, 3));
            _blockSensorTable.Add(new BlockSensorEntry(serial++, "p5v3", 1, 3));

            _scanToEmit = new double[session.GetNumberOfChannels()];

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
            _logger.Info($"Init success: {InstanceName} . Listening on { ThisDeviceSetup.LocalEndPoint.ToIpEp()}");

            Task.Factory.StartNew(() => OutputDeviceHandler_Task());
            _isDeviceReady = true;
            return true;
        }

        protected override void HandleRequest(EthernetMessage request)
        {
            byte[] byteMessage = request.GetByteArray(MessageWay.downstream);

            if (_isInDispose)
            {
                return;
            }

            // downstream message aimed to block sensor
            if (byteMessage[EthernetMessage._cardTypeOffset] == DeviceMap2.GetCardIdFromCardName(DeviceMap2.BlocksensorLiteral))
            {
                if (byteMessage.Length == 19) // is from digital card
                {
                    _subaddress = byteMessage[EthernetMessage._payloadOffset] & 0x7; // get lower 3 bits
                    return;
                }

                if (_subaddress < 0)
                {
                    _logger.Warn("Incoming block sensor message rejected. Reason: sub-address not defined");
                    return;
                }
                if ((_payloadLength + EthernetMessage._payloadOffset) != byteMessage.Length)
                {
                    _logger.Warn($"Incoming message length {byteMessage.Length}not match. expecting {_payloadLength + EthernetMessage._payloadOffset + _payloadLength}, message rejected.");
                    return;
                }
                // select entries from block-sensor table.
                var selectedEntries = _blockSensorTable.Where(ent => ent.Subaddress == this._subaddress);

                // convert incoming message 
                EthernetMessage downstreamEthMessage = EthernetMessage.CreateFromByteArray(byteMessage, MessageWay.downstream);
                double[] downstreamPayload = _analogConverter.DownstreamConvert(downstreamEthMessage.PayloadBytes) as double[];
                if (false == downstreamEthMessage.InternalValidityTest())
                {
                    _logger.Warn("Incoming message length validity check failed, message rejected.");
                }

                // build 'scan'
                Array.Clear(_scanToEmit, 0, _scanToEmit.Length);
                foreach (var entry in selectedEntries)
                {
                    double voltageToEmit = downstreamPayload[entry.EntrySerial];
                    int channelToEmit = entry.chan_ain;
                    _scanToEmit[channelToEmit] = voltageToEmit;
                }

                // emit 'scan' to analog card
                _analogWriter.WriteSingleScan(_scanToEmit);
            }

        }
        private AnalogConverter _analogConverter = new AnalogConverter(AI201100Setup.PeekVoltage_upstream, AO308Setup.PeekVoltage_downstream);
        const int _payloadLength = 28;

        public override void Enqueue(byte[] byteMessage)
        {
            // upstream message from digital/input card
            if (byteMessage[EthernetMessage._cardTypeOffset] == DeviceMap2.GetCardIdFromCardName(DeviceMap2.DIO403Literal)) //"DIO-403"))
            {
                // just fix message. to make it looks like downward message;
                byteMessage[0] = 0xaa;
                byteMessage[1] = 0x55;
                byteMessage[EthernetMessage._cardTypeOffset] = Convert.ToByte(DeviceMap2.GetCardIdFromCardName(DeviceMap2.BlocksensorLiteral));
                byteMessage[EthernetMessage._slotNumberOffset] = 32;
            }
            base.Enqueue(byteMessage);
        }

        /// <summary>
        /// Two types of messages might reach here
        /// 1. From "DIO-403/Input". (a copy of the upstream message)
        /// 2. From Ethernet. id of this message is 32 ("to BlockSensor").
        /// </summary>
        //public override void Enqueue(byte[] byteMessage)
        //{
        //    if (_isInDispose)
        //    {
        //        return;
        //    }
        //    // upstream message from digital/input card
        //    if (byteMessage[EthernetMessage._cardTypeOffset] == DeviceMap2.GetCardIdFromCardName(DeviceMap2.DIO403Literal)) //"DIO-403"))
        //    {
        //        // convert
        //        try
        //        {
        //            EthernetMessage msg = EthernetMessage.CreateFromByteArray(byteMessage, MessageWay.upstream);
        //            if (null == msg) throw new ArgumentNullException();

        //            _subaddress = msg.PayloadBytes[0] & 0x7; // get lower 3 bits
        //        }
        //        catch (ArgumentException ex)
        //        {
        //            _logger.Warn($"BlockSensor: {ex.Message}");
        //        }
        //        return;
        //    }

        //    // downstream message aimed to block sensor
        //    if (byteMessage[EthernetMessage._cardTypeOffset] == DeviceMap2.GetCardIdFromCardName( DeviceMap2.BlocksensorLiteral))
        //    {
        //        if (_subaddress < 0)
        //        {
        //            return;
        //        }
        //        if ((_payloadLength+EthernetMessage._payloadOffset) != byteMessage.Length)
        //        {
        //            _logger.Warn($"Incoming message length {byteMessage.Length}not match. expecting {_payloadLength+EthernetMessage._payloadOffset+_payloadLength}, message rejected.");
        //            return;
        //        }
        //        // select entries from block-sensor table.
        //        var selectedEntries = _blockSensorTable.Where(ent => ent.Subaddress == this._subaddress); 

        //        // convert incoming message 
        //        EthernetMessage downstreamEthMessage = EthernetMessage.CreateFromByteArray(byteMessage, MessageWay.downstream);
        //        double[] downstreamPayload = _analogConverter.DownstreamConvert(downstreamEthMessage.PayloadBytes) as double[];
        //        if (false == downstreamEthMessage.InternalValidityTest())
        //        {
        //            _logger.Warn("Incoming message length validity check failed, message rejected.");
        //        }

        //        // build 'scan'
        //        Array.Clear(_scanToEmit, 0, _scanToEmit.Length);
        //        foreach (var entry in selectedEntries)
        //        {
        //            double voltageToEmit = downstreamPayload[entry.EntrySerial];
        //            int channelToEmit = entry.chan_ain;
        //            _scanToEmit[channelToEmit] = voltageToEmit;
        //        }

        //        // emit 'scan' to analog card
        //        _analogWriter.WriteSingleScan(_scanToEmit);
        //    }
        //}

        public void Send(SendObject obj)
        {
            this.Enqueue(obj.ByteMessage);
        }

        public override void Dispose()
        {
            _isInDispose = true;
            _analogWriter = null;
            base.Dispose();

        }
    }
    class BlockSensorEntry
    {
        public int EntrySerial { get; private set; }
        public string SignalName { get; private set; }
        public int chan_ain { get; private set; }
        public int Subaddress { get; private set; }

        public BlockSensorEntry(int entrySerial, string signalName, int subaddress, int chan_ain)
        {
            this.EntrySerial = entrySerial;
            this.SignalName = signalName;
            this.Subaddress = subaddress;
            this.chan_ain = chan_ain;
        }
    }
}
