using System.Collections.Generic;
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


        public override string ToString()
        {
            return string.Format("databasepad: {0}\n templateliederen: {1}\n templatetheme: {2}\n regels per slide: {3}\n", FullDatabasePath, FullTemplateliederen, FullTemplatetheme, Regelsperslide);
        }
    }
}
