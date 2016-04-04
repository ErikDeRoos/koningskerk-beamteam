// Copyright 2016 door Remco Veurink en Erik de Roos
using System.Collections.Generic;
using System;
using System.Linq;

namespace ISettings.CommonImplementation
{
    public class Instellingen : IInstellingen
    {
        public string DatabasePad { get; set; }
        public string Templateliederen { get; set; }
        public string Templatetheme { get; set; }
        public string BijbelPad { get; set; }
        public int Regelsperslide { get; set; }
        private readonly List<IMapmask> _lijstmasks = new List<IMapmask>();
        public StandaardTeksten StandaardTeksten { get; set; }

        public Instellingen()
        {
            DatabasePad = "";
            BijbelPad = "";
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

        public Instellingen(string databasepad, string templateliederen, string templatetheme, string bijbelpad, int regelsperslide = 6, StandaardTeksten standaardTeksten = null, IEnumerable<IMapmask> masks = null)
            : this()
        {
            DatabasePad = databasepad;
            Templateliederen = templateliederen;
            Templatetheme = templatetheme;
            BijbelPad = bijbelpad;
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

        public string FullDatabasePath => DatabasePad.StartsWith(".") ? System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DatabasePad.Remove(0, 1)) : DatabasePad;
        public string FullBijbelPath => BijbelPad.StartsWith(".") ? System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BijbelPad.Remove(0, 1)) : BijbelPad;

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
