using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ByteStreamer3.Utilities
{
    public class AppState
    {
        public string PlayFolder { get; internal set; }
        public List<FileState> EntryStateList = new List<FileState>();

        public AppState(string playFolder)
        {
            PlayFolder = playFolder;
            //PlayFolderInfo = new DirectoryInfo(playFolder);
        }

        //public AppState() 
        //{
        //}

        internal FileState GetEntryState(FileInfo playFileInfo)
        {
            var ls = EntryStateList.Where( (FileState es) => es.Filename==playFileInfo.Name);
            return ls.FirstOrDefault();
        }
    }

    public class FileState
    {
        public bool IsChecked { get; set; }
        public string Filename { get; set; }

        public FileState(bool isChecked, string fname)
        {
            IsChecked = isChecked;
            Filename = fname;
        }

        public FileState() { }
    }

}
