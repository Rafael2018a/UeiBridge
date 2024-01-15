using System;
using System.Xml.Serialization;
using System.IO;

/// <summary>
/// All classes in this file MUST NOT depend on any other module in the project
/// </summary>
namespace UeiBridge.CubeSetupTypes
{
    /// <summary>
    /// Load/Save CubeSetup from/to xml file.
    /// CubeSetupMain property is the main object, 
    /// this might be null in case of failure.
    /// </summary>
    public class CubeSetupLoader
    {
        public CubeSetup CubeSetupMain { get; private set; }
        public CubeSetupLoader(FileInfo xmlFile)
        {
            if (!xmlFile.Exists)
            {
                return;
            }
            using (FileStream fs = xmlFile.OpenRead())
            {
                LoadSetupFile(fs);
            }
        }
        public CubeSetupLoader(Stream xmlStream)
        {
            LoadSetupFile(xmlStream);
        }
        CubeSetup LoadSetupFile(Stream xmlStream)
        {
            if ((null != CubeSetupMain) || (xmlStream == null))
            {
                throw new ArgumentException("null argument");
            }
            var serializer = new XmlSerializer(typeof(CubeSetup));
            CubeSetupMain = serializer.Deserialize(xmlStream) as CubeSetup;
            return CubeSetupMain;
        }
        public static bool SaveSetupFile(CubeSetup cs, FileInfo xmlFile)
        {
            if (null == cs)
            {
                throw new ArgumentException("null argument");
            }
            using (StreamWriter sw = new StreamWriter(xmlFile.Open(FileMode.Create, FileAccess.Write)))
            {
                bool rc = SaveSetupFile(cs, sw);
            }
            return true;
        }
        public static bool SaveSetupFile(CubeSetup cs, StreamWriter sw) // Stream xmlStream)
        {
            if ((null == cs) || (null == sw))
            {
                throw new ArgumentException("null argument");
            }

            var serializer = new XmlSerializer(typeof(CubeSetup));
            serializer.Serialize(sw, cs);

            return true;
        }
    }
}

