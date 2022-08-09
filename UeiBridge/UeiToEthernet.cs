using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UeiBridge
{
    class UeiToEthernet : IEnqueue<ScanResult> // tbd rename DeviceToEtheret 
    {
        ISend<byte[]> _destination;
        log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
        object _lockObject = new object();

        public UeiToEthernet(ISend<byte[]> destination)
        {
            _destination = destination;
        }

        public void Enqueue(ScanResult dr)
        {
            Task.Factory.StartNew(() => BuildAndSend_Task(dr));
        }

        void BuildAndSend_Task(ScanResult dr)
        {
            lock (_lockObject)  // tbd. use q
            {
                try
                {
                    //IConvert converter;
                    //if (ProjectRegistry.Instance.ConvertersDic.TryGetValue(dr.OriginDeviceName, out converter))
                    {
                        
                        byte[] payload = dr.OriginDevice.AttachedConverter.DeviceToEth(dr.Scan);

                        EthernetMessage mo = EthernetMessageFactory.CreateFromDevice(payload, dr.OriginDevice.DeviceName);
                        byte[] bytes = mo?.ToByteArrayUp();
                        _destination.Send(bytes);
                    }
                    //else
                    //{
                    //    _logger.Warn($"Can't find suitable converter for {dr.OriginDeviceName} (downward)");
                    //}
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to convert device result. " + ex.Message);
                }
            }
        }
    }
}
