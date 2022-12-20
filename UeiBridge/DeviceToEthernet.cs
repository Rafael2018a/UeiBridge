using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridgeTypes;

namespace UeiBridge
{
    [Obsolete]
    class DeviceToEthernet : IEnqueue<ScanResult>
    {
        ISend<byte[]> _destination;
        log4net.ILog _logger = log4net.LogManager.GetLogger("Root");
        object _lockObject = new object();
        BlockingCollection<ScanResult> _dataItemsQueue = new BlockingCollection<ScanResult>(100); // max 100 items

        public DeviceToEthernet(ISend<byte[]> destination)
        {
            _destination = destination;
        }
        public void Start()
        {
            Task.Factory.StartNew(() => BuildAndSend_Task());
        }

        public void Enqueue(ScanResult dr)
        {
            //Task.Factory.StartNew(() => BuildAndSend_Task(dr));
            _dataItemsQueue.Add(dr);
        }

        void BuildAndSend_Task()
        {
            while (false == _dataItemsQueue.IsCompleted)
            {
                try
                {
                    ScanResult dr = _dataItemsQueue.Take();

                    byte[] payload = dr.OriginDevice.AttachedConverter.DeviceToEth(dr.Scan);

                    EthernetMessage mo = EthernetMessage.CreateFromDevice(payload, dr.OriginDevice.DeviceName);
                    byte[] bytes = mo?.ToByteArrayUp();
                    _destination.Send(bytes);
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to convert device result. " + ex.Message);
                }
            }
        }
    }
}
