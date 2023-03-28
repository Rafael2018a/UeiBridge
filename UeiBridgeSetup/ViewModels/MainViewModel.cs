using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UeiBridgeSetup.ViewModels
{
    class MainViewModel: ViewModelBase
    {
        const string _defaultSetupFilename = "UeiSettings2.config";
        void LoadSetupFile( string fileFullpath)
        {
            FileInfo setupFileinfo = (fileFullpath == null) ? new FileInfo(_defaultSetupFilename) : new FileInfo(fileFullpath);
            if (!setupFileinfo.Exists)
            {
                return;
            }    

        }
    }
}
