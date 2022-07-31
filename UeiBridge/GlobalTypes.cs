using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

/// <summary>
/// All files in project might refer to this file.
/// Types in this file might NOT refer to types in any other file.
/// </summary>
namespace UeiBridge
{
    /// <summary>
    /// Send items that should be pushed to q (return immediatly)
    /// </summary>
    public interface IEnqueue<Item>
    {
        void Enqueue(Item i);
    }
    public interface ISend<Item>
    {
        void Send(Item i);
    }
    public abstract class OutputDevice 
    {
        //protected string _deviceIndex;
        protected string _channelsString;
        protected Session _deviceSession;
        protected string _caseUrl;
        protected string _deviceName;// = "AO-308";
        public string DeviceName => _deviceName;
        protected int _numberOfChannels = 0;
        //public int NumberOfChannels => _numberOfChannels;
        public virtual void CloseDevice()
        {
            if (null != _deviceSession)
            {
                _deviceSession.Stop();
                _deviceSession.Dispose();
            }
            _deviceSession = null;
        }
        public abstract void HandleRequest(DeviceRequest request);
    }
    public abstract class InputDevice 
    {
        
        protected Session _deviceSession;
        protected string _caseUrl;
        protected string _deviceName;// = "AO-308";
        protected int _numberOfChannels = 0;
        protected string _channelsString;
        public virtual void CloseDevice()
        {
            if (null != _deviceSession)
            {
                _deviceSession.Stop();
                _deviceSession.Dispose();
            }
            _deviceSession = null;
        }
    }
    public interface IConvert
    {
        object EthToDevice(byte[] messagePayload);
        byte[] DeviceToEth(object dt);
        string DeviceName { get; }
    }
    public class DeviceRequest
    {
        readonly object _requestObject;
        readonly string _caseUrl;
        readonly string _deviceName;

        public object RequestObject => _requestObject;

        public string CaseUrl => _caseUrl;
        public string DeviceName => _deviceName;
        public DeviceRequest(object requestObject, string caseUrl, string deviceName)
        {
            _requestObject = requestObject;
            _caseUrl = caseUrl;
            _deviceName = deviceName;
        }
    }
    public class DeviceResponse
    {
        object _response;
        string _originDeviceName;

        public DeviceResponse(object response, string originDeviceName)
        {
            _response = response;
            _originDeviceName = originDeviceName;
        }

        public object Response { get => _response; }
        public string OriginDeviceName { get => _originDeviceName; }
    }
}
