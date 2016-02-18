using ISettings;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace PowerpointGenerater.Settings
{
    public class SettingsFactory : IInstellingenFactory
    {
        private readonly string _baseDir;
        private readonly string _instellingenFileName;
        private readonly string _masksFileName;

        public SettingsFactory(string instellingenFileName, string masksFileName)
        {
            _baseDir = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            _instellingenFileName = instellingenFileName;
            _masksFileName = masksFileName;
        }
        private static bool WriteToXmlFile(string instellingenFile, string maskFile, Instellingen instellingen)
        {
            try
            {
                //schrijf instellingen weg
                var serializer = new XmlSerializer(typeof(Instellingen));
                using (TextWriter sw = new StreamWriter(instellingenFile))
                {
                    serializer.Serialize(sw, instellingen);
                    sw.Flush();
                }

                //schrijf Masks weg
                using (var xw = XmlWriter.Create(maskFile, new XmlWriterSettings() { Indent = true }))
                {
                    xw.WriteStartDocument();
                    xw.WriteStartElement("Masks");
                    foreach (var mask in instellingen.Masks)
                    {
                        xw.WriteStartElement("Mask");
                        xw.WriteStartElement("Name");
                        xw.WriteString(mask.Name);
                        xw.WriteEndElement();
                        xw.WriteStartElement("RealName");
                        xw.WriteString(mask.RealName);
                        xw.WriteEndElement();
                        xw.WriteEndElement();
                    }
                    xw.WriteEndElement();
                    xw.WriteEndDocument();

                    xw.Flush();
                }

                return true;
            }
            catch (PathTooLongException)
            {
                return false;
            }
        }

        public bool WriteToXmlFile(IInstellingen instellingen)
        {
            return WriteToXmlFile(Path.Combine(_baseDir, _instellingenFileName), Path.Combine(_baseDir, _masksFileName), (instellingen as Instellingen) ?? GetDefault(_baseDir));
        }

        private static Instellingen GetDefault(string baseDir)
        {
            return new Instellingen(
                (baseDir + @"Resources\Database"), 
                (baseDir + @"Resources\Database\Template Liederen.pptx"), 
                (baseDir + @"Resources\Database\Achtergrond.pptx")
            );
        }


        private static Instellingen LoadFromXmlFile(string instellingenFile, string maskFile)
        {
            Instellingen instellingen;

            if (!File.Exists(instellingenFile))
                return null;

            var serializer = new XmlSerializer(typeof(Instellingen));
            var settings = new XmlReaderSettings();

            using (var textReader = new StreamReader(instellingenFile))
            {
                using (var xmlReader = XmlReader.Create(textReader, settings))
                {
                    instellingen = serializer.Deserialize(xmlReader) as Instellingen;
                }
            }
            if (instellingen == null)
                return null;

            if (!File.Exists(maskFile))
                return instellingen;

            var xdoc = new XmlDocument();
            xdoc.Load(maskFile);
            var root = xdoc.DocumentElement;

            if (root == null) return instellingen;
            var masklist = root.GetElementsByTagName("Mask");
            foreach (XmlNode mask in masklist)
            {
                var nameNode = mask.SelectSingleNode("Name");
                var realnameNode = mask.SelectSingleNode("RealName");
                if (nameNode != null && realnameNode != null)
                    instellingen.AddMask(new Mapmask(nameNode.InnerText, realnameNode.InnerText));
            }

            return instellingen;
        }

        public IInstellingen LoadFromXmlFile()
        {
            return LoadFromXmlFile(Path.Combine(_baseDir, _instellingenFileName), Path.Combine(_baseDir, _masksFileName)) ?? GetDefault(_baseDir);
        }
    }
}
