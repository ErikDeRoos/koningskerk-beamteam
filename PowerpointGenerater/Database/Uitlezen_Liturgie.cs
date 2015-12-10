using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PowerpointGenerater.Database
{
    /// <summary>
    /// Maak een ruwe lijst van een tekstuele liturgie
    /// </summary>
    /// <remarks>Gebruikt standaard scheidingstekens</remarks>
    class InterpreteerLiturgieRuw
    {
        private static readonly char[] _benamingScheidingstekens = new char[] { ':' };
        private static readonly char[] _benamingDeelScheidingstekens = new char[] { ' ' };
        private static readonly char[] _versScheidingstekens = new char[] { ',' };

        /// <summary>
        /// Leest de tekstuele invoer in en maakt er een ruwe liturgie lijst van
        /// </summary>
        public IEnumerable<ILiturgieOnderdeelRuw> VanTekstregels(String[] regels)
        {
            return regels
              .Where(r => !String.IsNullOrWhiteSpace(r))
              .Select(r => VanTekstregel(r))
              .Where(r => r != null)
              .ToList();
        }

        private Onderdeel VanTekstregel(String invoer)
        {
            var regel = new Onderdeel();
            var invoerTrimmed = invoer.Trim();
            var voorBenamingStukken = invoerTrimmed.Split(_benamingScheidingstekens, StringSplitOptions.RemoveEmptyEntries);
            if (voorBenamingStukken.Length == 0)
                return null;
            var preBenamingTrimmed = voorBenamingStukken[0].Trim();
            // Een benaming kan uit delen bestaan, bijvoorbeeld 'psalm 110' in 'psalm 110:1,2' of 'opwekking 598' in 'opwekking 598'
            var voorPreBenamingStukken = preBenamingTrimmed.Split(_benamingDeelScheidingstekens, StringSplitOptions.RemoveEmptyEntries);
            if (voorPreBenamingStukken.Length > 1)
                regel.Deel = voorPreBenamingStukken[voorPreBenamingStukken.Length - 1];  // Is altijd laatste deel
            regel.Benaming = preBenamingTrimmed.Substring(0, preBenamingTrimmed.Length - (regel.Deel ?? "").Length).Trim();
            // Verzen als '1,2' in 'psalm 110:1,2'
            regel.Verzen = (voorBenamingStukken.Length > 1 ? voorBenamingStukken[1] : "")
              .Split(_versScheidingstekens, StringSplitOptions.RemoveEmptyEntries)
              .Select(v => v.Trim())
              .ToList();
            return regel;
        }

        class Onderdeel : ILiturgieOnderdeelRuw
        {
            public String Benaming { get; set; }
            public String Deel { get; set; }
            public IEnumerable<String> Verzen { get; set; }
        }
    }

    /// <summary>
    /// Ruwe liturgie regels, zoals ze ingevoerd zijn
    /// </summary>
    public interface ILiturgieOnderdeelRuw
    {
        String Benaming { get; }
        String Deel { get; }
        IEnumerable<String> Verzen { get; }
    }


    /// <summary>
    /// Interpreteer de ruwe liturgie regels tot zoekacties
    /// </summary>
    class InterpreteerLiturgieZoekacie
    {
        private static readonly char[] _benamingSplitScheidingstekens = new char[] { ' ' };
        private readonly IEnumerable<Mapmask> _masks;
        public InterpreteerLiturgieZoekacie(IEnumerable<Mapmask> gebruikMasks)
        {
            _masks = gebruikMasks;
        }

        /// <summary>
        /// Interpreteer de ruwe liturgie regels icm intelligentie zoals masks en hardcoded afkortingen.
        /// </summary>
        public IEnumerable<ILiturgieOnderdeelZoekactie> VanOnderdelen(IEnumerable<ILiturgieOnderdeelRuw> regels)
        {
            return regels
              .Select(r => VanOnderdeel(r))
              .Where(r => r != null)
              .ToList();
        }

        private Onderdeel VanOnderdeel(ILiturgieOnderdeelRuw invoer)
        {
            var regel = new Onderdeel();
            regel.Type = LiturgieType.EnkelZonderDeel;
            regel.Ruw = invoer;
            regel.EchteBenaming = invoer.Benaming;
            // Bepaal basis pad. Deze is relatief: bewust nog geen database pad ervoor geplaatst. Dat gebeurd pas bij inlezen.
            var basisPad = "" + Path.DirectorySeparatorChar;

            // Bepaal hoe de liturgie regel in delen is opgedeeld
            if (!String.IsNullOrWhiteSpace(regel.Ruw.Deel))
                regel.Type = LiturgieType.EnkelMetDeel;
            // Het is mogelijk een bestand in een map aan te wijzen door spaties te gebruiken. Ook dit oplossen
            var benamingOnderdelen = invoer.Benaming.Split(_benamingSplitScheidingstekens, StringSplitOptions.RemoveEmptyEntries);
            if (benamingOnderdelen.Length > 1)
            {
                regel.EchteBenaming = benamingOnderdelen[benamingOnderdelen.Length - 1];  // Laatste is echte naam
                basisPad +=
                  String.Join("", benamingOnderdelen.Select((o, i) => new { Naam = o, Index = i })
                    .Where(o => o.Index != benamingOnderdelen.Length - 1)
                    .Select(o => o.Naam + Path.DirectorySeparatorChar));
            }
            // liturgie regel met verzen is de uitgebreidste variant
            if (invoer.Verzen.Any())
                regel.Type = LiturgieType.MeerMetDeel;

            // Bepaal nu de zoekhint
            if (regel.Type == LiturgieType.MeerMetDeel)
            {
                regel.Type = LiturgieType.MeerMetDeel;
                // de verzen zijn de te zoeken items. De rest is pad
                basisPad += invoer.Benaming + Path.DirectorySeparatorChar;
                if (!String.IsNullOrEmpty(invoer.Deel))
                    basisPad += invoer.Deel + Path.DirectorySeparatorChar;
                //deel de verschillende bestandsnamen op(er zijn verschillende verzen mogelijk uit één map bijvoorbeeld)
                regel.ZoekactieHints = invoer.Verzen.Select(v => new OnderdeelHint() { Nummer = v, ZoekPad = basisPad + v }).ToList();
            }
            else if (regel.Type == LiturgieType.EnkelMetDeel)
            {
                // Benaming deel is het gezochte item, rest is pad
                regel.ZoekactieHints = new[] { new OnderdeelHint() { ZoekPad = basisPad + invoer.Benaming + Path.DirectorySeparatorChar + invoer.Deel } };
            }
            else {
                // Benaming is gezochte item
                regel.ZoekactieHints = new[] { new OnderdeelHint() { ZoekPad = basisPad + invoer.Benaming } };
            }

            // Virtuele benaming is de 'mooie' benaming die op het liturgie bord /volgende verschijnt. Maar
            // hij wordt 'normaal' ingevoerd. Deze virtuele naam is een 'mask' en daar dan ook te vinden
            // TODO verplaatsen? virtuele benaming is pas relevant in liturgie bord generator
            regel.VirtueleBenaming = regel.EchteBenaming;
            // Check of er een mask op de benaming zit
            var maskCheck = _masks.FirstOrDefault(m => String.Compare(m.RealName, regel.EchteBenaming, true) == 0);
            if (maskCheck != null)
                regel.VirtueleBenaming = maskCheck.Name;
            return regel;
        }

        class Onderdeel : ILiturgieOnderdeelZoekactie
        {
            public ILiturgieOnderdeelRuw Ruw { get; set; }
            public String VirtueleBenaming { get; set; }
            public String EchteBenaming { get; set; }
            public LiturgieType Type { get; set; }
            public IEnumerable<ILiturgieOnderdeelZoekactieHint> ZoekactieHints { get; set; }
        }
        class OnderdeelHint : ILiturgieOnderdeelZoekactieHint
        {
            public String Nummer { get; set; }
            public String ZoekPad { get; set; }
        }
    }

    /// <summary>
    /// Ruwe liturgie regels, aangevuld met zoekactie hints (filesystem paden)
    /// </summary>
    public interface ILiturgieOnderdeelZoekactie
    {
        ILiturgieOnderdeelRuw Ruw { get; }
        String VirtueleBenaming { get; }
        String EchteBenaming { get; }
        LiturgieType Type { get; }
        IEnumerable<ILiturgieOnderdeelZoekactieHint> ZoekactieHints { get; }
    }
    public interface ILiturgieOnderdeelZoekactieHint
    {
        String Nummer { get; }
        String ZoekPad { get; }
    }
    public enum LiturgieType
    {
        /// <summary>
        /// Enkelvoudige aanduiding, bijvoorbeeld een openingsslide.
        /// </summary>
        EnkelZonderDeel,
        /// <summary>
        /// Enkelvoudige aanduiding met deel benaming, bijvoorbeeld een database 
        /// zoals opwekking waar wel nummers zijn maar geen verzen
        /// </summary>
        EnkelMetDeel,
        /// <summary>
        /// Meervoudige aanduiding met deel benaming, bijvoorbeeld een database
        /// zoals psalmen waar nummers zijn met individuele verzen
        /// </summary>
        MeerMetDeel,
    }


    /// <summary>
    /// Zoek naar de opgegeven liturgieen.
    /// </summary>
    class LiturgieDatabase
    {
        private readonly String _databasePad;
        public LiturgieDatabase(String databasePad)
        {
            _databasePad = databasePad;
            if (!_databasePad.EndsWith(Path.DirectorySeparatorChar + ""))
                _databasePad += Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Zoek, in dit geval in het bestandssysteem, naar de opgegeven liturgieen.
        /// We zoeken uit hoe de liturgie opgeslagen is (tekst, powerpoint) en geven terug
        /// wat we op kunnen halen.
        public IEnumerable<ILiturgieZoekresultaat> Zoek(IEnumerable<ILiturgieOnderdeelZoekactie> zoekopdracht)
        {
            return zoekopdracht
              .Select(z => Zoek(z))
              .ToList();
        }

        private ILiturgieZoekresultaat Zoek(ILiturgieOnderdeelZoekactie zoekopdracht)
        {
            var resultaat = new ZoekresultaatItem();
            resultaat.Type = zoekopdracht.Type;
            resultaat.Resultaten = zoekopdracht.ZoekactieHints.Select(zh => ZoekDeel(zh)).ToList();
            resultaat.EchteBenaming = zoekopdracht.EchteBenaming;
            resultaat.DeelBenaming = zoekopdracht.Ruw.Deel;
            resultaat.VirtueleBenaming = zoekopdracht.VirtueleBenaming;
            return resultaat;
        }

        private ILiturgieZoekresultaatDeel ZoekDeel(ILiturgieOnderdeelZoekactieHint deelHint)
        {
            var resultaat = new ZoekresultaatDeelItem();
            resultaat.Zoekopdracht = deelHint.ZoekPad;
            resultaat.Nummer = deelHint.Nummer;
            resultaat.Gevonden = false;
            var bestandsPadExtensieloos = _databasePad + deelHint.ZoekPad;

            //probeer eerst powerpoint bestanden
            var path = bestandsPadExtensieloos + ".pptx";
            if (File.Exists(path))
            {
                resultaat.Gevonden = true;
                resultaat.InhoudType = InhoudType.PptLink;
                resultaat.Inhoud = path;
                return resultaat;
            }
            path = bestandsPadExtensieloos + ".ppt";
            if (File.Exists(path))
            {
                resultaat.Gevonden = true;
                resultaat.InhoudType = InhoudType.PptLink;
                resultaat.Inhoud = path;
                return resultaat;
            }
            path = bestandsPadExtensieloos + ".txt";
            if (File.Exists(path))
            {
                resultaat.Gevonden = true;
                //probeer om tekst te lezen van bestand
                try
                {
                    resultaat.Inhoud = LeesTekstBestand(path);
                }
                catch
                {
                    resultaat.Gevonden = false;
                }
                resultaat.InhoudType = InhoudType.Tekst;
                return resultaat;
            }
            return resultaat;
        }

        private String LeesTekstBestand(String bestandsNaam)
        {
            //open een filestream naar het gekozen bestand
            var strm = new FileStream(bestandsNaam, FileMode.Open, FileAccess.Read);
            //gebruik streamreader om te lezen van de filestream
            using (var rdr = new StreamReader(strm))
            {
                //return de liturgie
                return rdr.ReadToEnd();
            }
        }

        class ZoekresultaatItem : ILiturgieZoekresultaat
        {
            public LiturgieType Type { get; set; }
            public String VirtueleBenaming { get; set; }
            public String EchteBenaming { get; set; }
            public String DeelBenaming { get; set; }
            public IEnumerable<ILiturgieZoekresultaatDeel> Resultaten { get; set; }
        }
        class ZoekresultaatDeelItem : ILiturgieZoekresultaatDeel
        {
            public String Nummer { get; set; }
            public String Zoekopdracht { get; set; }
            public Boolean Gevonden { get; set; }
            public String Inhoud { get; set; }
            public InhoudType InhoudType { get; set; }
        }
    }

    /// <summary>
    /// Zoekresultaat item(s) samen met de zoekopdracht gegevens
    /// </summary>
    public interface ILiturgieZoekresultaat
    {
        LiturgieType Type { get; }
        String VirtueleBenaming { get; }
        String EchteBenaming { get; }
        String DeelBenaming { get; }
        IEnumerable<ILiturgieZoekresultaatDeel> Resultaten { get; }
    }
    /// <summary>
    /// Het gezochte item en of/wat er gevonden is
    /// </summary>
    public interface ILiturgieZoekresultaatDeel
    {
        String Nummer { get; }
        String Zoekopdracht { get; }
        Boolean Gevonden { get; }
        String Inhoud { get; }
        InhoudType InhoudType { get; }
    }
    public enum InhoudType
    {
        Tekst,
        PptLink,
    }
}
