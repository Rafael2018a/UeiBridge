﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteStreamer3.Utilities
{
    public class JFileHeader
    {
        public bool EnablePlay { get; set; } = false;
        public string DestIp { get; set; } = "227.10.20.30";
        public int DestPort { get; set; } = 50099;
        public int NumberOfCycles { get; set; }= 20;
        public int WaitStateMs { get; set; }= 100;
        public string ConverterName { get; set; }

        public JFileHeader()
        {
        }
    }
    public class JFileBody
    {
        public int CardType { get; set; } = 2;
        public int SlotNumber { get; set; } = 1;
        public int CubeId { get; set; } = 2;
        public int[] Payload { get; set; }

        public JFileBody(int[] payload)
        {
            this.Payload = payload;
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
