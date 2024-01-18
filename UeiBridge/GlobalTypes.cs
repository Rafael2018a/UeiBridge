using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UeiDaq;

/// <summary>
/// All files in project might refer to this file.
/// Types in this file might NOT refer to types in any other file.
/// </summary>
namespace UeiBridge.Library.Types
{

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
