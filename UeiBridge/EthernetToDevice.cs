using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace UeiBridge
{
    /// <summary>
    /// Convert byte[] to DeviceRequest objects and sends to to appropriate device manager.
    /// </summary>
    public class EthernetToDevice : IEnqueue<byte[]> 
    {
        BlockingCollection<byte[]> _dataItemsQueue = new BlockingCollection<byte[]>(100);
        log4net.ILog _logger = log4net.LogManager.GetLogger("Root");

        public void Start()
        {
            Task.Factory.StartNew(() => EthToDevice_Task());
        }
#if dont
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
                    if (ProjectRegistry.Instance.DeviceKeys.TryGetValue(message.CardType, out deviceName)) // get deviceManager for card-type
                    {
                        _logger.Debug($"Ethernet Message accepted. Device:{deviceName} Payload length:{message.PayloadBytes.Length}");
                        DeviceRequest dreq = MakeDeviceRequest2(message, deviceName); 

                        if (null != dreq)
                        {
                            OutputDevice deviceManager;
                            if (ProjectRegistry.Instance.DeviceManagersDic.TryGetValue(deviceName, out deviceManager))
                            {
                                deviceManager.Enqueue(dreq);
                                //Task.Factory.StartNew( () => deviceManager.HandleRequest(dreq));
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
#endif
        void EthToDevice_Task()
        {
            while (false == _dataItemsQueue.IsCompleted)
            {
                // get from q
                byte[] incomingMessage = _dataItemsQueue.Take();

                try
                {
                    // convert byte[] to messageObject
                    string errorString;
                    EthernetMessage messageObj = EthernetMessageFactory.CreateFromByteArray(incomingMessage, out errorString);
                    if (null == messageObj)
                    {
                        _logger.Warn(errorString);
                        continue;
                    }

                    //locate device manager and asks him to  handle DeviceRequest 
                    string deviceName;
                    if (ProjectRegistry.Instance.DeviceKeys.TryGetValue(messageObj.CardType, out deviceName)) // get deviceManager for card-type
                    {
                        //_logger.Debug($"Ethernet Message accepted. Device:{deviceName} Payload length:{messageObj.PayloadBytes.Length}");
                        OutputDevice deviceManager = ProjectRegistry.Instance.OutputDevicesMap[deviceName];

                        DeviceRequest dreq = MakeDeviceRequest(messageObj, deviceManager.AttachedConverter);
                        deviceManager.Enqueue(dreq);
                    }
                    else
                    {
                        _logger.Warn($"Can't find device key for card type {messageObj.CardType}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex.Message);
                }
            }
        }
#if old
        private DeviceRequest MakeDeviceRequest_old(EthernetMessage msg, string deviceName)
        {
            // convert
            IConvert converter;
            if (false == ProjectRegistry.Instance.ConvertersDic.TryGetValue(deviceName, out converter))
            {
                _logger.Warn("MakeDeviceRequest - Can't find converter");
                return null;
            }

            var payload = converter.EthToDevice(msg.PayloadBytes);
            if (null != payload)
            {
                DeviceRequest dr = new DeviceRequest(payload, Config.Instance.DeviceUrl, deviceName);
                return dr;
            }
            else
            {
                _logger.Warn($"MakeDeviceRequest - Convert fail. Reason: {converter.LastErrorMessage}");
                return null;
            }
        }
#endif
        private DeviceRequest MakeDeviceRequest(EthernetMessage messageObject, IConvert converter)
        {
            if (null==converter)
            {
                _logger.Warn($"MakeDeviceRequest - Convert fail. Reason: null converter");
                return null;
            }

            var devicePayload = converter.EthToDevice( messageObject.PayloadBytes);
            if (null != devicePayload)
            {
                DeviceRequest dr = new DeviceRequest(devicePayload, Config.Instance.DeviceUrl, messageObject.SlotChannelNumber);
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