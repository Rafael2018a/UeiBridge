using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteStreame3.Utilities
{
    public class JsonMessageHeader
    {
        public string Title { get => title; set => title = value; }
        public int NumberOfCycles { get => numberOfCycles; set => numberOfCycles = value; }
        public int WaitStateMs { get => waitStateMs; set => waitStateMs = value; }
        public string DestIp { get => destIp; set => destIp = value; }
        public int DestPort { get => destPort; set => destPort = value; }
        //public string ContaingFile { get => containgFile; set => containgFile = value; }

        private string title = "Uei general card";
        private int numberOfCycles = 20;
        private int waitStateMs = 100;
        private string destIp = "227.10.20.30";
        private int destPort = 50099;
        //private string containgFile;

        public JsonMessageHeader()
        {
        }
    }
    public class JsonMessageBody
    {
        public int CardId = 5;
        public int SlotNumber = 0;
        public int[] Payload { get; set; }

        public JsonMessageBody(int[] payload)
        {
            Payload = payload;
        }
    }
    public class JsonMessage
    {
        public bool EnablePlay { get; set; }
        public JsonMessageHeader Header { get; set; }
        public JsonMessageBody Body { get; set; }

        public JsonMessage(JsonMessageHeader header, JsonMessageBody body)
        {
            EnablePlay = false;
            Header = header;
            Body = body;
        }
    }

}
