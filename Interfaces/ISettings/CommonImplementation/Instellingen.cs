using System.Collections.Generic;
using System;
using System.Linq;

namespace ISettings.CommonImplementation
{
    public class Instellingen : IInstellingen
    {
        public string Databasepad { get; set; }
        public string Templateliederen { get; set; }
        public string Templatetheme { get; set; }
        public int Regelsperslide { get; set; }
        private readonly List<IMapmask> _lijstmasks = new List<IMapmask>();
        public StandaardTeksten StandaardTeksten { get; set; }

        public Instellingen()
        {
            Databasepad = "";
            Templateliederen = "";
            Templatetheme = "";
            Regelsperslide = 4;

            StandaardTeksten = new StandaardTeksten()
            {
                Volgende = "Straks :",
                Voorganger = "Voorganger :",
                Collecte1 = "1e collecte :",
                Collecte2 = "2e collecte :",
                Collecte = "Collecte :",
                Lezen = "Lezen :",
                Tekst = "Tekst :",
                Liturgie = "liturgie",
                LiturgieLezen = "L ",
                LiturgieTekst = "T "
            };

        }

        public Instellingen(string databasepad, string templateliederen, string templatetheme, int regelsperslide = 6, StandaardTeksten standaardTeksten = null, IEnumerable<IMapmask> masks = null)
            : this()
        {
            Databasepad = databasepad;
            Templateliederen = templateliederen;
            Templatetheme = templatetheme;
            Regelsperslide = regelsperslide;
            if (standaardTeksten != null)
                StandaardTeksten = standaardTeksten;
            masks?.ToList().ForEach(m => AddMask(m));
        }

        public bool AddMask(IMapmask mask)
        {
            if (_lijstmasks.Contains(mask)) return false;
            _lijstmasks.Add(mask);
            return true;
        }
        public void ClearMasks()
        {
            _lijstmasks.Clear();
        }

        public string FullDatabasePath => Databasepad.StartsWith(".") ? System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Databasepad.Remove(0, 1)) : Databasepad;

        public string FullTemplatetheme => Templatetheme.StartsWith(".") ? System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Templatetheme.Remove(0, 1)) : Templatetheme;

        public string FullTemplateliederen => Templateliederen.StartsWith(".") ? System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Templateliederen.Remove(0, 1)) : Templateliederen;

        public IEnumerable<IMapmask> Masks => _lijstmasks;


        public override string ToString()
        {
            return
                $"databasepad: {FullDatabasePath}\n templateliederen: {FullTemplateliederen}\n templatetheme: {FullTemplatetheme}\n regels per slide: {Regelsperslide}\n";
        }
    }
}
