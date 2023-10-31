using System;
using System.Xml.Serialization;
using System.IO;

/// <summary>
/// All classes in this file MUST NOT depend on any other module in the project
/// </summary>
namespace UeiBridge.CubeSetupTypes
{
    public class CubeSetupLoader
    {
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
        public CubeSetup CubeSetupMain { get; private set; }
        CubeSetup LoadSetupFile(FileInfo xmlFile)
        {
            if (!xmlFile.Exists)
            {
                return null;
            }
            using(FileStream fs = xmlFile.OpenRead())
            {
                return LoadSetupFile(fs);
            }
        }
        CubeSetup LoadSetupFile(Stream xmlStream)
        {
            if ((null != CubeSetupMain)||(xmlStream==null))
            {
                throw new ArgumentException("null argument");
            }
            var serializer = new XmlSerializer(typeof(CubeSetup));
            try
            {
                CubeSetupMain = serializer.Deserialize(xmlStream) as CubeSetup;
            }
            catch(InvalidOperationException ex) // bad formatted xml
            {
                return null;
            }

            return CubeSetupMain;
        }
        public static bool SaveSetupFile(CubeSetup cs, FileInfo xmlFile)
        {
            if (null==cs)
            {
                throw new ArgumentException("null argument");
            }
            using( FileStream fs = xmlFile.OpenWrite())
            {
                return SaveSetupFile(cs, fs);
            }
        }
        public static bool SaveSetupFile(CubeSetup cs, Stream xmlStream)
        {
            if ((null == cs) || (null == xmlStream))
            {
                throw new ArgumentException("null argument");
            }

            var serializer = new XmlSerializer( typeof(CubeSetup));
            serializer.Serialize(xmlStream, cs);

            return true;
        }
    }
}
