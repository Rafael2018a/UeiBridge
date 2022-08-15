using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatusViewer
{
    static class AppServices
    {
        public static void WriteToTrace(string format, params string[] args)
        {
            string decoratedString = string.Format("StatusViewer: " + format, args);

            System.Diagnostics.Trace.WriteLine(decoratedString);
        }
    }
}
