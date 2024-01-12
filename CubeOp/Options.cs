using System.Collections.Generic;
using CommandLine;

namespace CubeOp
{
    // see https://stackoverflow.com/questions/66494408/commandlineparser-show-help-results-if-no-switch-is-given

    class Options
    {
        [Option('r', "read", HelpText = "Input files to be processed.")]
        public IEnumerable<string> InputFiles { get; set; }

        // Omitting long name, defaults to name of property, ie "--verbose"
        [Option(Default = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        //[Option(Default = true, HelpText = "cube id")]
        //public string stdin { get; set; }

        //[Value(0, MetaName = "offset", HelpText = "File offset.")]
        //public long? Offset { get; set; }

        //[Option("cubeid", HelpText = "IP of cube or 'simu' for simulation")]
        //public string cubeid { get; set; }

        [Option("ping-cube", HelpText = "WD reset for specified cube")]
        public bool pingcube { get; set; }
        [Option("reset-cube", HelpText = "WD reset for specified cube")]
        public bool resetcube { get; set; }

        [Option("list-cubes", HelpText = "Get list of connected cubes")]
        public bool listcubes { get; set; }

        [Option('?', HelpText = "Help")]
        public bool dohelp { get; set; }

        [Value(0, MetaName = "cube_id", HelpText = "IP of cube or 'simu' for simulation")]
        public string cube_id { get; set; }

    }
}