using System;
using UeiBridge.Library.Types;
using UeiBridge.Library;
using System.Collections.Generic;
using System.Linq;
using UeiBridge.Library.CubeSetupTypes;
using UeiBridge.Library.Interfaces;

namespace UeiBridge
{
    public class BlockSensorManager2: AO308OutputDeviceManager, ISend<SendObject>
    {
        #region === publics ====
        public override string DeviceName => DeviceMap2.BlocksensorLiteral;
        #endregion
        #region === privates ===
        //private log4net.ILog _logger = StaticMethods.GetLogger();
        private List<BlockSensorEntry> _blockSensorTable = new List<BlockSensorEntry>();
        //private IWriterAdapter<double[]> _analogWriter;
        private int _subaddress = -1;
        private double[] _scanToEmit;
        //private BlockSensorSetup _thisDeviceSetup;
        private log4net.ILog _logger = StaticLocalMethods.GetLogger();
        //private bool _isInDispose = false;
        BlockSensorSetup _thisDeviceSetup;
        #endregion
        const int _payloadLength = 28;

        public BlockSensorManager2( BlockSensorSetup deviceSetup1, ISession session) 
            : base( deviceSetup1 as AO308Setup, session, true)
        {
            _thisDeviceSetup = deviceSetup1;
            //System.Diagnostics.Debug.Assert(analogWriter != null);
            //this._analogWriter = analogWriter;

            //_thisDeviceSetup = deviceSetup1 as BlockSensorSetup;
            //System.Diagnostics.Debug.Assert(null != _thisDeviceSetup);

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
        public BlockSensorManager2() {}
        protected override void HandleRequest(EthernetMessage request)
        {
            byte[] byteMessage = request.GetByteArray(MessageWay.downstream);

            if (_inDisposeState)
            {
                return;
            }

            // downstream message aimed to block sensor
            if (byteMessage[EthernetMessage._cardTypeOffset] == DeviceMap2.GetDeviceName(DeviceMap2.BlocksensorLiteral))
            {
                if (byteMessage.Length == 22) // is from digital card
                {
                    _subaddress = byteMessage[EthernetMessage._payloadOffset] & 0x7; // get lower 3 bits
                    return;
                }

                // verify valid sub-address
                if (_subaddress < 0)
                {
                    _logger.Warn("Incoming block sensor message rejected. Reason: sub-address not defined");
                    return;
                }
                // verify length
                if ((_payloadLength + EthernetMessage._payloadOffset) != byteMessage.Length)
                {
                    _logger.Warn($"Incoming message length {byteMessage.Length} not match. expecting {_payloadLength + EthernetMessage._payloadOffset + _payloadLength}, message rejected.");
                    return;
                }
                // select entries from block-sensor table.
                var selectedEntries = _blockSensorTable.Where(ent => ent.Subaddress == this._subaddress);

                // convert incoming message 
                string err=null;
                EthernetMessage downstreamEthMessage = EthernetMessage.CreateFromByteArray(byteMessage, MessageWay.downstream, ref err);
                if (null==downstreamEthMessage)
                {
                    _logger.Warn(err);
                    return;
                }
                double[] downstreamPayload = _attachedConverter.DownstreamConvert(downstreamEthMessage.PayloadBytes) as double[];
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

        public override void Enqueue(byte[] byteMessage)
        {
            // upstream message from digital/input card
            if (byteMessage[EthernetMessage._cardTypeOffset] == DeviceMap2.GetDeviceName(DeviceMap2.DIO403Literal)) //"DIO-403"))
            {
                // just fix message. to make it looks like downward message;
                byteMessage[0] = 0xaa;
                byteMessage[1] = 0x55;
                byteMessage[EthernetMessage._cardTypeOffset] = Convert.ToByte(DeviceMap2.GetDeviceName(DeviceMap2.BlocksensorLiteral));
                byteMessage[EthernetMessage._slotNumberOffset] = Convert.ToByte( _thisDeviceSetup.SlotNumber);
            }
            base.Enqueue(byteMessage);
        }

        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            return new string[] { "block sensor not ready yet" };
        }

        public void Send(SendObject obj)
        {
            this.Enqueue(obj.ByteMessage);
        }
    }
}