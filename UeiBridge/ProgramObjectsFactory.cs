using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using log4net;
using UeiBridge.Types;

namespace UeiBridge
{
    class ProgramObjectsFactory
    {
        private List<List<PerDeviceObjects>> _deviceObjectsTable;
        ILog _logger = StaticMethods.GetLogger();
        public ProgramObjectsFactory(List<List<PerDeviceObjects>> deviceObjectsTable)
        {
            this._deviceObjectsTable = deviceObjectsTable;
        }
    }
}
