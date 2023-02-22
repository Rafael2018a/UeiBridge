using System;
using System.IO;
using ByteStreame3.Utilities;
using Newtonsoft.Json;


namespace ByteStreamer3
{
    /// <summary>
    /// Holds all available info on single play item
    /// (json file name, the class that represents file contents...)
    /// </summary>
    internal class PlayItem
    {
        #region == publics ==
        public PlayItemJson PlayObject => _playObject; 
        public FileInfo PlayFile => _playFile;
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
        public bool IsValidItem { get { return ((_playObject != null) && (_playObject.Body != null) && (_playObject.Header != null));  } }
        #endregion
        #region == privates ==
        readonly FileInfo _playFile;
        PlayItemJson _playObject;
        int _playedBlockCount;
        #endregion

        public PlayItem(FileInfo fileToPlay)
        {
            this._playFile = fileToPlay;
            using (StreamReader reader = _playFile.OpenText())
            {
                _playObject = JsonConvert.DeserializeObject<PlayItemJson>(reader.ReadToEnd());
            }
            EthMessage = JsonToEtherentMessage(_playObject);
        }

        private UeiBridge.Library.EthernetMessage JsonToEtherentMessage(PlayItemJson playItem)
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
