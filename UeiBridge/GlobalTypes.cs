using System.Collections.Generic;
using System.Linq;
using System.Text;
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
}
