using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
    public interface IConvert
    {
        object EthToDevice(byte[] messagePayload);
        byte[] DeviceToEth(object dt);
        string DeviceName { get; }
        string LastErrorMessage { get; }
    }
    /// <summary>
    /// (Immutable)
    /// </summary>
    [Obsolete]
    public class DeviceRequest
    {
        readonly object _requestObject;
        readonly string _caseUrl;
        readonly int _serialChannel;
        //readonly string _deviceName;

        public object RequestObject => _requestObject;

        public string CaseUrl => _caseUrl;

        public int SerialChannel => _serialChannel;

        //public string DeviceName => _deviceName;
        public DeviceRequest(object requestObject, string caseUrl, int serialChannel = -1)//, string deviceName=null)
        {
            _requestObject = requestObject;
            _caseUrl = caseUrl;
            _serialChannel = serialChannel;
        }
    }

    /// <summary>
    /// Contains: Object to write to device, serial channel id (in case of serial)
    /// </summary>
    public class ScanResult
    {
        object _scan;
        InputDevice _originDevice;
        public ScanResult(object scan, InputDevice originDevice)
        {
            _scan = scan;
            _originDevice = originDevice;
        }
        public object Scan { get => _scan; }
        public InputDevice OriginDevice { get => _originDevice; }
    }

    class PerDeviceObjects
    {
        public InputDevice _inputDeviceManager;
        public OutputDevice _outputDeviceManager;
        public UdpReader _udpReader;
        public UdpWriter _udpWriter;

        public PerDeviceObjects(InputDevice inputDevice, UdpWriter udpWriter)
        {
            _inputDeviceManager = inputDevice;
            _udpWriter = udpWriter;
        }
        public void NewObjects(InputDevice inputDevice, UdpWriter udpWriter)
        {
            System.Diagnostics.Debug.Assert(_inputDeviceManager == null);
            System.Diagnostics.Debug.Assert(_udpWriter == null);
            _inputDeviceManager = inputDevice;
            _udpWriter = udpWriter;
        }

        public PerDeviceObjects(OutputDevice outputDevice, UdpReader udpReader)
        {
            _outputDeviceManager = outputDevice;
            _udpReader = udpReader;
        }
        public void NewObjects(OutputDevice outputDevice, UdpReader udpReader)
        {
            System.Diagnostics.Debug.Assert(null == _outputDeviceManager);
            System.Diagnostics.Debug.Assert(null == _udpReader);
            _outputDeviceManager = outputDevice;
            _udpReader = udpReader;
        }
    }
    public class EndPoint
    {
        public string Address;
        public int Port;
        public EndPoint()
        {
        }
        public EndPoint(string addressString, int port)
        {
            Address = addressString;
            Port = port;
        }
        public IPEndPoint ToIpEp() // tbd. make cast operator
        {
            //try
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(Address), Port);
                return ipep;
            }
            //catch (Exception ex)
            //{
            //    return null;
            //}
        }
    }
    public class SendObject
    {
        public IPEndPoint TargetEndPoint { get; }
        public byte[] ByteMessage { get; }
        public SendObject(IPEndPoint targetEndPoint, byte[] byteMessage)
        {
            TargetEndPoint = targetEndPoint;
            ByteMessage = byteMessage;
        }
    }

    public interface IDeviceManager
    {
        string DeviceName { get; }
    }

}
