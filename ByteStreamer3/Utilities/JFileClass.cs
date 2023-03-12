using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteStreamer3.Utilities
{
    public class JFileHeader
    {
        public bool EnablePlay { get; set; }
        public string Title { get => title; set => title = value; }
        public string ConverterName { get; set; } // 
        public int NumberOfCycles { get => numberOfCycles; set => numberOfCycles = value; }
        public int WaitStateMs { get => waitStateMs; set => waitStateMs = value; }
        public string DestIp { get => destIp; set => destIp = value; }
        public int DestPort { get => destPort; set => destPort = value; }

        private string title = "Uei general card";
        private int numberOfCycles = 20;
        private int waitStateMs = 100;
        private string destIp = "227.10.20.30";
        private int destPort = 50099;

        public JFileHeader()
        {
        }
    }
    public class JFileBody
    {
        public int CardId = 5;
        public int SlotNumber = 0;
        public int[] Payload { get; set; }

        public JFileBody(int[] payload)
        {
            Payload = payload;
        }
    }
    public class JFileClass
    {
        public JFileHeader Header { get; set; }
        public JFileBody Body { get; set; }

        public JFileClass(JFileHeader header, JFileBody body)
        {
            Header = header;
            Body = body;

        }
    }

}
