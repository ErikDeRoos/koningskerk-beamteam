// Copyright 2016 door Remco Veurink en Erik de Roos
using IFileSystem;
using ISettings;
using ISettings.CommonImplementation;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace PowerpointGenerator.Settings
{
    public class SettingsFactory : IInstellingenFactory
    {
        private readonly string _baseDir;
        private readonly string _instellingenFileName;
        private readonly string _masksFileName;
        private IFileOperations _fileManager;

        public SettingsFactory(IFileOperations fileManager, string instellingenFileName, string masksFileName)
        {
            _fileManager = fileManager;
            _baseDir = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);  // TODO alternatief voor vinden
            _instellingenFileName = instellingenFileName;
            _masksFileName = masksFileName;
        }

        public bool WriteToXmlFile(IInstellingen instellingen)
        {
            return WriteToXmlFile(_fileManager, _fileManager.CombineDirectories(_baseDir, _instellingenFileName), _fileManager.CombineDirectories(_baseDir, _masksFileName), (instellingen as Instellingen) ?? GetDefault());
        }

        public IInstellingen LoadFromXmlFile()
        {
            return LoadFromXmlFile(_fileManager, _fileManager.CombineDirectories(_baseDir, _instellingenFileName), _fileManager.CombineDirectories(_baseDir, _masksFileName)) ?? GetDefault();
        }

        private static bool WriteToXmlFile(IFileOperations fileManager, string instellingenFile, string maskFile, Instellingen instellingen)
        {
            try
            {
                // verwijder oude bestand (zelfde effect als overschreven worden)
                if (fileManager.FileExists(instellingenFile))
                    fileManager.Delete(instellingenFile);

                //schrijf instellingen weg
                var serializer = new XmlSerializer(typeof(Instellingen));
                using (var sw = new StreamWriter(fileManager.FileWriteStream(instellingenFile)))
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

        private static Instellingen LoadFromXmlFile(IFileOperations fileManager, string instellingenFile, string maskFile)
        {
            Instellingen instellingen;

            if (!fileManager.FileExists(instellingenFile))
                return null;

            var serializer = new XmlSerializer(typeof(Instellingen));
            var settings = new XmlReaderSettings();

            using (var textReader = new StreamReader(fileManager.FileReadStream(instellingenFile)))
            {
                using (var xmlReader = XmlReader.Create(textReader, settings))
                {
                    instellingen = serializer.Deserialize(xmlReader) as Instellingen;
                }
            }
            if (instellingen == null)
                return null;

            if (!fileManager.FileExists(maskFile))
                return instellingen;

            using (var maskStream = fileManager.FileReadStream(maskFile))
            {
                var xdoc = new XmlDocument();
                xdoc.Load(maskStream);
                var root = xdoc.DocumentElement;

                if (root == null)
                    return instellingen;
                var masklist = root.GetElementsByTagName("Mask");
                foreach (XmlNode mask in masklist)
                {
                    var nameNode = mask.SelectSingleNode("Name");
                    var realnameNode = mask.SelectSingleNode("RealName");
                    if (nameNode != null && realnameNode != null)
                        instellingen.AddMask(new Mapmask(nameNode.InnerText, realnameNode.InnerText));
                }
            }

            return instellingen;
        }

        private static Instellingen GetDefault()
        {
            return new Instellingen() {
                DatabasePad = @".Resources\Database",
                BijbelPad = @".Resources\Bijbels\NBV",
                TemplateTheme = @".Resources\Database\Achtergrond.pptx",
                TemplateLied = @".Resources\Database\Template Liederen.pptx",
                TemplateBijbeltekst = @".Resources\Database\Template Bijbeltekst.pptx",
            };
        }
    }
}
