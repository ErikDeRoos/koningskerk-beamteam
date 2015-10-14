using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;

namespace PowerpointGenerater
{
    public class Instellingen
    {
        public string Databasepad;
        public string Templateliederen;
        public string Templatetheme;
        public int Regelsperslide = 4;
        private List<Mapmask> lijstmasks = new List<Mapmask>();
        public StandaardTeksten StandaardTekst { get; set; }

        public Instellingen()
            : this("", "", "",4)
        {

        }

        public Instellingen(string databasepad, string templateliederen, string templatetheme, int regelsperslide)
        {
            this.Databasepad = databasepad;
            this.Templateliederen = templateliederen;
            this.Templatetheme = templatetheme;
            this.Regelsperslide = regelsperslide;
            this.StandaardTekst = new StandaardTeksten()
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

        public bool AddMask(Mapmask mask)
        {
            if (!lijstmasks.Contains(mask))
            {
                lijstmasks.Add(mask);
                return true;
            }
            return false;
        }

        public List<Mapmask> GetMasks()
        {
            return lijstmasks;
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
                    return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Databasepad.Remove(0,1));

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
        
        public static bool WriteXML(Instellingen instellingen, string path)
        {
            try
            {
                XmlWriterSettings xws = new XmlWriterSettings();
                xws.Indent = true;

                //schrijf instellingen weg
                XmlWriter xw; 

                XmlSerializer serializer = new XmlSerializer(typeof(Instellingen));
                using (TextWriter sw = new StreamWriter(path + "instellingen.xml"))
                {
                    serializer.Serialize(sw, instellingen);
                }
               
                //schrijf Masks weg
                xw = XmlWriter.Create(path + "masks.xml", xws);
                xw.WriteStartDocument();
                    xw.WriteStartElement("Masks");
                    foreach (Mapmask mask in instellingen.lijstmasks)
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
                xw.Close();

                return true;
            }catch (PathTooLongException)
            {
                return false;
            }
        }

        public static Instellingen LoadXML(string path)
        {
            Instellingen instellingen = new Instellingen();
            XmlDocument xdoc = new XmlDocument();

            

            //XmlNodeList nodelist = root.GetElementsByTagName("Name");
            //XmlNodeList nodelist2 = root.GetElementsByTagName("RealName");
            //if (nodelist.Count == nodelist2.Count)
            //{
            //    for (int i = 0; i < nodelist.Count; i++)
            //    {
            //        instellingen.lijstmasks.Add(new Mapmask(nodelist[i].InnerText, nodelist2[i].InnerText));
            //    }
            //}

            string fileName = path + "instellingen.xml";
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Instellingenbestand niet gevonden", fileName);

            XmlSerializer serializer = new XmlSerializer(typeof(Instellingen));
            XmlReaderSettings settings = new XmlReaderSettings();
            // No settings need modifying here

            using (StreamReader textReader = new StreamReader(fileName))
            {
                using (XmlReader xmlReader = XmlReader.Create(textReader, settings))
                {
                    instellingen = (Instellingen)serializer.Deserialize(xmlReader);
                }
            }

            xdoc.Load(path + "masks.xml");
            XmlElement root = xdoc.DocumentElement;

            XmlNodeList masklist = root.GetElementsByTagName("Mask");
            foreach (XmlNode mask in masklist)
            {
                XmlNode nameNode = mask.SelectSingleNode("Name");
                XmlNode realnameNode = mask.SelectSingleNode("RealName");
                instellingen.lijstmasks.Add(new Mapmask(nameNode.InnerText, realnameNode.InnerText));
            }

            return instellingen;
        }

        public override string ToString()
        {
            return string.Format("databasepad: {0}\n templateliederen: {1}\n templatetheme: {2}\n regels per slide: {3}\n", FullDatabasePath, FullTemplateliederen, FullTemplatetheme, Regelsperslide);
        }
    }
}
