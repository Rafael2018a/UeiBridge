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
        public JFileClass JFileObject => _jFileObject; 
        public FileInfo PlayFileInfo => _playFile;
        public UeiBridge.Library.EthernetMessage EthMessage { get; set; }
        public bool IsValidItem { get { return ((_jFileObject != null) && (_jFileObject.Body != null) && (_jFileObject.Header != null));  } }
        public System.Net.IPEndPoint DestEndPoint => _destEp;
        //{ get { return new System.Net.IPEndPoint(System.Net.IPAddress.Parse(_jFileObject.Header.DestIp), _jFileObject.Header.DestPort); } }
        #endregion

        #region == privates ==
        readonly FileInfo _playFile;
        JFileClass _jFileObject;
        UdpWriter _udpWriter;
        System.Net.IPEndPoint _destEp;
        //int _playedBlockCount;
        #endregion

        public void SendBlockByUdp()
        {
            _udpWriter.Send(EthMessage.GetByteArray( UeiBridge.Library.MessageWay.downstream));
        }
        public PlayFile(FileInfo fileToPlay)
        {
            this._playFile = fileToPlay;
            try
            {
                using (StreamReader reader = _playFile.OpenText())
                {
                    _jFileObject = JsonConvert.DeserializeObject<JFileClass>(reader.ReadToEnd());
                    _destEp = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(_jFileObject.Header.DestIp), _jFileObject.Header.DestPort);
                    _udpWriter = new UdpWriter(_destEp);
                }
                EthMessage = JsonToEtherentMessage(_jFileObject);
            }
            catch(Exception ex) when (ex is JsonReaderException || ex is JsonSerializationException || ex is NullReferenceException)
            {
                _jFileObject = null;
                EthMessage = null;
            }
        }

        private UeiBridge.Library.EthernetMessage JsonToEtherentMessage(JFileClass playItem)
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
