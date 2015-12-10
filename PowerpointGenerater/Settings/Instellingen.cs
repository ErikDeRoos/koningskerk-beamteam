using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;
using ISettings;

namespace PowerpointGenerater
{
    public class Instellingen : IInstellingen
    {
        public string Databasepad { get; set; }
        public string Templateliederen { get; set; }
        public string Templatetheme { get; set; }
        public int Regelsperslide { get; set; }
        private List<IMapmask> lijstmasks = new List<IMapmask>();
        private StandaardTeksten _standaardTeksten;
        public IStandaardTeksten StandaardTeksten { get { return _standaardTeksten; } }

        public Instellingen()
        {
            Databasepad = "";
            Templateliederen = "";
            Templatetheme = "";
            Regelsperslide = 4;
            _standaardTeksten = new StandaardTeksten()
            {
                Volgende = "Straks :",
                Voorganger = "Voorganger :",
                Collecte1 = "1e collecte :",
                Collecte2 = "2e collecte :",
                Collecte = "Collecte :",
                Lezen = "Lezen :",
                Tekst = "Tekst :",
                Liturgie = "liturgie"
            };

        }

        public Instellingen(string databasepad, string templateliederen, string templatetheme, int regelsperslide = 6, StandaardTeksten standaardTeksten = null)
            : this()
        {
            Databasepad = databasepad;
            Templateliederen = templateliederen;
            Templatetheme = templatetheme;
            Regelsperslide = regelsperslide;
            if (standaardTeksten != null)
                _standaardTeksten = standaardTeksten;
        }

        public bool AddMask(IMapmask mask)
        {
            if (!lijstmasks.Contains(mask))
            {
                lijstmasks.Add(mask);
                return true;
            }
            return false;
        }
        public void ClearMasks()
        {
            lijstmasks.Clear();
        }

        public string FullDatabasePath
        {
            get
            {
                if (Databasepad.StartsWith("."))
                    return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Databasepad.Remove(0, 1));

                return Databasepad;
            }
        }

        public string FullTemplatetheme
        {
            get
            {
                if (Templatetheme.StartsWith("."))
                    return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Templatetheme.Remove(0, 1));

                return Templatetheme;
            }
        }

        public string FullTemplateliederen
        {
            get
            {
                if (Templateliederen.StartsWith("."))
                    return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Templateliederen.Remove(0, 1));

                return Templateliederen;
            }
        }

        public IEnumerable<IMapmask> Masks { get { return lijstmasks; } }

        public bool WriteToXMLFile(string path)
        {
            try
            {
                //schrijf instellingen weg
                var serializer = new XmlSerializer(typeof(Instellingen));
                using (TextWriter sw = new StreamWriter(path + "instellingen.xml"))
                {
                    serializer.Serialize(sw, this);
                    sw.Flush();
                }

                //schrijf Masks weg
                using (var xw = XmlWriter.Create(path + "masks.xml", new XmlWriterSettings() { Indent = true }))
                {
                    xw.WriteStartDocument();
                    xw.WriteStartElement("Masks");
                    foreach (var mask in Masks)
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

        public static Instellingen LoadFromXMLFile(string path)
        {
            var instellingen = (Instellingen)null;

            string fileName = path + "instellingen.xml";
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Instellingenbestand niet gevonden", fileName);

            var serializer = new XmlSerializer(typeof(Instellingen));
            var settings = new XmlReaderSettings();
            // No settings need modifying here

            using (var textReader = new StreamReader(fileName))
            {
                using (var xmlReader = XmlReader.Create(textReader, settings))
                {
                    instellingen = serializer.Deserialize(xmlReader) as Instellingen;
                }
            }
            if (instellingen == null)
                return instellingen;

            var xdoc = new XmlDocument();
            xdoc.Load(path + "masks.xml");
            var root = xdoc.DocumentElement;

            var masklist = root.GetElementsByTagName("Mask");
            foreach (XmlNode mask in masklist)
            {
                var nameNode = mask.SelectSingleNode("Name");
                var realnameNode = mask.SelectSingleNode("RealName");
                instellingen.AddMask(new Mapmask(nameNode.InnerText, realnameNode.InnerText));
            }

            return instellingen;
        }


        public override string ToString()
        {
            return string.Format("databasepad: {0}\n templateliederen: {1}\n templatetheme: {2}\n regels per slide: {3}\n", FullDatabasePath, FullTemplateliederen, FullTemplatetheme, Regelsperslide);
        }
    }
}
