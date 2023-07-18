using System;
using System.IO;
using ByteStreamer3.Utilities;
using Newtonsoft.Json;


namespace ByteStreamer3
{
    /// <summary>
    /// JFileClass object wrapper.
    /// Open, Save, Verify validity
    /// </summary>
    public class JFileAux
    {
        public JFileClass JFileObject { get; private set; } = null;//=> _jFileObject; 
        public FileInfo PlayFileInfo { get; private set; } = null;

        public bool IsValidItem()
        {
            return ((JFileObject != null) && (JFileObject.Body != null) && (JFileObject.Header != null) && (JFileObject.Body.Payload != null));
        }

        public JFileAux(FileInfo fileToPlay)
        {
            this.PlayFileInfo = fileToPlay;
            if (!fileToPlay.Exists)
            {
                return;
            }
            try
            {
                using (StreamReader reader = PlayFileInfo.OpenText())
                {
                    JFileObject = JsonConvert.DeserializeObject<JFileClass>(reader.ReadToEnd());
                }
            }
            catch(Exception ex) when (ex is JsonReaderException || ex is JsonSerializationException || ex is NullReferenceException)
            {
                // nothing to do here (JFileObject is already null)
            }
        }

        public static UeiBridge.Library.EthernetMessage JsonToEthernetMessage(JFileClass playItem)
        {
            byte[] block = new byte[playItem.Body.Payload.Length];
            Buffer.BlockCopy(playItem.Body.Payload, 0, block, 0, block.Length);
            for(int i=0; i< playItem.Body.Payload.Length; i++)
            {
                block[i] = Convert.ToByte(playItem.Body.Payload[i]);
            }
            UeiBridge.Library.EthernetMessage resultMessage = UeiBridge.Library.EthernetMessage.CreateMessage(playItem.Body.CardType, playItem.Body.SlotNumber, playItem.Body.CubeId, block);
            return resultMessage;
        }

        internal void Save()
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            using (StreamWriter sw = PlayFileInfo.CreateText())
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, this.JFileObject);
                }
            }
        }

    }
}
