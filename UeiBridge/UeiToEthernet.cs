using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UeiBridge
{
    class UeiToEthernet : IEnqueue<DeviceResponse>
    {
        ISend<byte[]> _destination;
        log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
        object _lockObject = new object();

        public UeiToEthernet(ISend<byte[]> destination)
        {
            _destination = destination;
        }

        public void Enqueue(DeviceResponse dr)
        {
            Task.Factory.StartNew(() => BuildAndSend_Task(dr));
        }

        void BuildAndSend_Task(DeviceResponse dr)
        {
            
            lock (_lockObject)
            {
                try
                {
                    IConvert converter;
                    if (ProjectRegistry.Instance.ConvertersDic.TryGetValue(dr.OriginDeviceName, out converter))
                    {
                        byte[] payload = converter.DeviceToEth(dr.Response);

                        EthernetMessage mo = EthernetMessageFactory.CreateFromDevice(payload, dr.OriginDeviceName);
                        byte[] bytes = mo?.ToByteArrayUp();
                        _destination.Send(bytes);
                    }
                    else
                    {
                        _logger.Warn($"Can't find suitable converter for {dr.OriginDeviceName} (downward)");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to convert device result. " + ex.Message);
                }
            }
        }
    }
}
