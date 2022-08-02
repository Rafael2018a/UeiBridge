using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace UeiBridge
{
    /// <summary>
    /// Convert byte[] to DeviceRequest objects and sends to to appropriate device manager.
    /// </summary>
    public class EthernetToUei : IEnqueue<byte[]>
    {
        BlockingCollection<byte[]> _dataItemsQueue = new BlockingCollection<byte[]>(100); // max 100 items
        log4net.ILog _logger = StaticMethods.GetLogger();

        public void Start()
        {
            Task.Factory.StartNew(() => EthToUei_Task());
        }

        /// <summary>
        /// Starts internal long-live-thread
        /// </summary>
        void EthToUei_Task()
        {
            while (false == _dataItemsQueue.IsCompleted)
            {
                // get from q
                byte[] incomingMessage = _dataItemsQueue.Take();

                try
                {
                    // convert byte[] to messageObject
                    string errorString;
                    EthernetMessage message = EthernetMessageFactory.CreateFromByteArray(incomingMessage, out errorString);
                    if (null == message)
                    {
                        _logger.Warn(errorString);
                        continue;
                    }

                    //locate device manager and asks him to  handle DeviceRequest 
                    string deviceName;
                    if (ProjectRegistry.Instance.DeviceKeys.TryGetValue(message.CardType, out deviceName))
                    {
                        _logger.Debug($"Ethernet Message accepted. Device:{deviceName} Payload length:{message.PayloadBytes.Length}");
                        DeviceRequest dreq = MakeDeviceRequest(message, deviceName);

                        if (null != dreq)
                        {
                            OutputDevice deviceManager;
                            if (ProjectRegistry.Instance.DeviceManagersDic.TryGetValue(deviceName, out deviceManager))
                            {
                                Task.Factory.StartNew( () => deviceManager.HandleRequest(dreq));
                            }
                            else
                            {
                                _logger.Warn($"Can't find device manager for key {deviceName}");
                            }
                        }
                    }
                    else
                    {
                        _logger.Warn($"Can't find device key for card type {message.CardType}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex.Message);
                }
            }
        }

        private DeviceRequest MakeDeviceRequest(EthernetMessage msg, string deviceName)
        {
            // convert
            IConvert converter;
            if (false == ProjectRegistry.Instance.ConvertersDic.TryGetValue(deviceName, out converter))
            {
                _logger.Warn("MakeDeviceRequest - Can't find converter");
                return null;
            }

            var req = converter.EthToDevice(msg.PayloadBytes);
            if (null != req)
            {
                DeviceRequest dr = new DeviceRequest(req, Config.Instance.DeviceUrl , deviceName);
                return dr;
            }
            else
            {
                _logger.Warn($"MakeDeviceRequest - Convert fail. Reason: {converter.LastErrorMessage}");
                return null;
            }
        }

        public void Enqueue(byte[] byteMessage)
        {
            _dataItemsQueue.Add(byteMessage);
        }
    }
}