using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UeiDaq;

/// <summary>
/// All files in project might refer to this file.
/// Types in this file might NOT refer to types in any other file.
/// </summary>
namespace UeiBridge.Types
{
    /// <summary>
    /// Send items that should be pushed to q (return immediately)
    /// </summary>
    public interface IEnqueue<Item>
    {
        void Enqueue(Item i);
    }
    public interface ISend<Item>
    {
        void Send(Item i);
    }
    //public interface IConvert
    //{
    //    object EthToDevice(byte[] messagePayload);
    //    byte[] DeviceToEth(object dt);
    //    string DeviceName { get; }
    //    string LastErrorMessage { get; }
    //}
    public interface IConvert2<SourceType>
    {
        SourceType DownstreamConvert(byte[] messagePayload);
        byte[] UpstreamConvert(SourceType dt);
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
    //public class ScanResult
    //{
    //    object _scan;
    //    UeiBridge.InputDevice _originDevice; 
    //    public ScanResult(object scan, UeiBridge.InputDevice originDevice)
    //    {
    //        _scan = scan;
    //        _originDevice = originDevice;
    //    }
    //    public object Scan { get => _scan; }
    //    public UeiBridge.InputDevice OriginDevice { get => _originDevice; }
    //}
    /// <summary>
    /// This class encapsulates payload with dest address
    /// </summary>
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
        string InstanceName { get; }
        string [] GetFormattedStatus( TimeSpan interval);
    }

    //public class TeeObject : ISend<SendObject>
    //{
    //    IEnqueue<byte[]> send1;
    //    ISend<SendObject> send2;
    //    public TeeObject(IEnqueue<byte[]> send1, ISend<SendObject> send2)
    //    {
    //        this.send1 = send1;
    //        this.send2 = send2;
    //    }
    //    public void Send(SendObject obj)
    //    {
    //        send1.Enqueue(obj.ByteMessage);
    //        send2.Send(obj);
    //    }
    //}

    /// <summary>
    /// Helper class for GetFormattedStatus() method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ViewItem<T>
    {
        public T readValue;
        public TimeSpan timeToLive;

        public ViewItem(T readValue, int timeToLiveMs)
        {
            this.readValue = readValue;
            this.timeToLive = TimeSpan.FromMilliseconds(timeToLiveMs);
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
