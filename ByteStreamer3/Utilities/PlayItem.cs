using System;
using System.IO;
using ByteStreamer3.Utilities;
using Newtonsoft.Json;


namespace ByteStreamer3
{
    /// <summary>
    /// Holds all available info on single play item
    /// (json file name, the class that represents file contents...)
    /// </summary>
    public class PlayFile
    {
        #region == publics ==
        public JItem PlayObject => _jObject; 
        public FileInfo PlayFileInfo => _playFile;
        //public byte[] BytesBlock => _bytesBlock;
        public int PlayedBlockCount 
        {
            get { return _playedBlockCount; }
            set
            {
                _playedBlockCount = value;
                if (PlayedBlockCountEvent!=null)
                {
                    PlayedBlockCountEvent(PlayedBlockCount);
                }
            }
        }
        public event Action<int> PlayedBlockCountEvent;
        public UeiBridge.Library.EthernetMessage EthMessage { get; set; }
        public bool IsValidItem { get { return ((_jObject != null) && (_jObject.Body != null) && (_jObject.Header != null));  } }
        #endregion
        #region == privates ==
        readonly FileInfo _playFile;
        JItem _jObject;
        int _playedBlockCount;
        #endregion

        public PlayFile(FileInfo fileToPlay)
        {
            this._playFile = fileToPlay;
            try
            {
                using (StreamReader reader = _playFile.OpenText())
                {
                    _jObject = JsonConvert.DeserializeObject<JItem>(reader.ReadToEnd());
                }
                EthMessage = JsonToEtherentMessage(_jObject);
            }
            catch(JsonSerializationException)
            {
                _jObject = null;
                EthMessage = null;
            }
        }

        private UeiBridge.Library.EthernetMessage JsonToEtherentMessage(JItem playItem)
        {
            if (!IsValidItem)
            {
                return null;
            }
            byte[] block = new byte[playItem.Body.Payload.Length];
            Buffer.BlockCopy(playItem.Body.Payload, 0, block, 0, block.Length);
            UeiBridge.Library.EthernetMessage resultMessage = UeiBridge.Library.EthernetMessage.CreateMessage(playItem.Body.CardId, playItem.Body.SlotNumber, 0, block);
            return resultMessage;
        }
    }
}
