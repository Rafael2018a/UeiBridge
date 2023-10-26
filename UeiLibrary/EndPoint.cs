using System;
using System.Net;

/// <summary>
/// All classes in this file MUST NOT depend on any other module in the project
/// </summary>
namespace UeiBridge.Library
{
    /// <summary>
    /// This serializable class represents ip address+port pair.
    /// </summary>
    public class EndPoint : IEquatable<EndPoint>
    {
        public string Address { get;  set; }
        public int Port { get;  set; }
        public EndPoint()
        {
        }
        public EndPoint(string addressString, int port)
        {
            Address = addressString;
            Port = port;
        }
        public void SetAddress(string add)
        {
            IPAddress ip;
            bool ok = IPAddress.TryParse(Address, out ip);
            if (ok)
            {
                Address = add;
            }
        }
        public void SetPort(int port)
        {
            if (port>=0)
            {
                this.Port = port;
            }
        }
        public bool Equals(EndPoint other)
        {
            return (this.Address == other.Address) && (this.Port == other.Port);
        }
        public IPEndPoint ToIpEp() 
        {
            IPAddress ip;
            bool ok = IPAddress.TryParse(Address, out ip);
            if (ok)
            {
                IPEndPoint ipep = new IPEndPoint(ip, Port);
                return ipep;
            }
            else
            {
                return null;
            }
        }
        public static EndPoint MakeEndPoint(string addressString, int port)
        {
            EndPoint ep = new EndPoint(addressString, port);

            if (null != ep.ToIpEp())
            {
                return ep;
            }
            else
            {
                return null;
            }
        }
    }
}
