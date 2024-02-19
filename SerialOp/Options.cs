﻿using System.Collections.Generic;
using CommandLine;

namespace SerialOp
{
    // see https://stackoverflow.com/questions/66494408/commandlineparser-show-help-results-if-no-switch-is-given

    class Options
    {
        //[Option('r', "read", HelpText = "Input files to be processed.")]
        //public IEnumerable<string> InputFiles { get; set; }

        //// Omitting long name, defaults to name of property, ie "--verbose"
        //[Option(Default = false, HelpText = "Prints all messages to standard output.")]
        //public bool Verbose { get; set; }

        //[Option(Default = true, HelpText = "cube id")]
        //public string stdin { get; set; }

        //[Value(0, MetaName = "offset", HelpText = "File offset.")]
        //public long? Offset { get; set; }

        //[Option("cubeid", HelpText = "IP of cube or 'simu' for simulation")]
        //public string cubeid { get; set; }

        [Option("rx", HelpText = "Run as receiver")]
        public bool rx { get; set; }
        [Option("tx", HelpText = "Run as transmitter")]
        public bool tx { get; set; }
        //[Option("reset-cube", HelpText = "WD reset for specified cube")]
        //public bool resetcube { get; set; }
        //[Option("reset-device", HelpText = "Reset specified device")]
        //public bool resetdevice { get; set; }
        //[Option("slot", HelpText = "Slot number (0 based)", Default =-1)]
        //public int slotnumber { get; set; }
        //[Option("list-cubes", HelpText = "Get list of connected cubes")]
        //public bool listcubes { get; set; }
        //[Option("list-devices", HelpText = "Get list of devices in specified cube")]
        //public bool listdevices { get; set; }
        //[Option("open-serial", HelpText = "Try to open serial device (use cube-id as resource-string")]
        //public bool openserial { get; set; }
        [Option('?', HelpText = "Help")]
        public bool dohelp { get; set; }
        [Value(0, MetaName = "device-uri", HelpText = "Uri of device, like pdna://192.168.100.3/Dev0")]
        public string DeviceUri { get; set; }

    }
}