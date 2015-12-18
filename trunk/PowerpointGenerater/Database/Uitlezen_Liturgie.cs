using IDatabase;
using ILiturgieDatabase;
using ISettings;
using PowerpointGenerater.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PowerpointGenerator.Database {

    // TODO Stap 1: database via XML setting files per dir maken (editor? nee)
    // TODO Stap 2: intelligentie voor on-the-fly uitzoeken database (vooral als hulp voor x tot y vraagstukken en voorbereiding voor assistentie)
    // TODO Stap 3: assistentie bij invullen liturgie
    // TODO mask weer ergens toepassen







    /// <summary>
    /// Maak een ruwe lijst van een tekstuele liturgie
    /// </summary>
    class InterpreteerLiturgieRuw : ILiturgieInterpreteer
    {
        private static readonly char[] _benamingScheidingstekens = new char[] { ':' };
        private static readonly char[] _benamingDeelScheidingstekens = new char[] { ' ' };
        private static readonly char[] _versScheidingstekens = new char[] { ',' };
        private static readonly char[] _optieStart = new char[] { '(' };
        private static readonly char[] _optieEinde = new char[] { ')' };
        private static readonly char[] _optieScheidingstekens = new char[] { ',' };

        private static Interpretatie SplitTekstregel(string invoer)
        {
            var regel = new Interpretatie();
            var invoerTrimmed = invoer.Trim();
            var voorOpties = invoerTrimmed.Split(_optieStart, StringSplitOptions.RemoveEmptyEntries);
            if (voorOpties.Length == 0)
                return null;
            var opties = voorOpties.Length > 1 ? voorOpties[1].Split(_optieEinde, StringSplitOptions.RemoveEmptyEntries)[0].Trim() : String.Empty;
            var voorBenamingStukken = voorOpties[0].Trim().Split(_benamingScheidingstekens, StringSplitOptions.RemoveEmptyEntries);
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
            regel.Opties = (!string.IsNullOrEmpty(opties) ? opties : "")
              .Split(_optieScheidingstekens, StringSplitOptions.RemoveEmptyEntries)
              .Select(v => v.Trim())
              .ToList();
            return regel;
        }

        public ILiturgieInterpretatie VanTekstregel(string regels)
        {
            return SplitTekstregel(regels);
        }

        /// <summary>
        /// Leest de tekstuele invoer in en maakt er een ruwe liturgie lijst van
        /// </summary>
        public IEnumerable<ILiturgieInterpretatie> VanTekstregels(string[] regels)
        {
            return regels
              .Where(r => !string.IsNullOrWhiteSpace(r))
              .Select(r => VanTekstregel(r))
              .Where(r => r != null)
              .ToList();
        }

        private class Interpretatie : ILiturgieInterpretatie
        {
            public string Benaming { get; set; }
            public string Deel { get; set; }

            public IEnumerable<string> Opties { get; set; }
            public IEnumerable<string> Verzen { get; set; }
        }
    }

    
    static class LiturgieDatabaseSettings
    {
        public const string OptieNietVerwerken = "geendb";
        public const string OptieNietTonenInVolgende = "geenvolg";
        public const string OptieNietTonenInOverzicht = "geenlt";
        public const string OptieAlternatieveNaamOverzicht = "altlt";
        public const string OptieAlternatieveNaam = "altnm";
        public const string VersSamenvoeging = "-";
    }



    /// <summary>
    /// Zoek naar de opgegeven liturgieen.
    /// </summary>
    class LiturgieDatabase : ILiturgieLosOp
    {
        private readonly IEngine<FileEngineSetSettings> _database;
        public LiturgieDatabase(IEngine<FileEngineSetSettings> database)
        {
            _database = database;
        }


        public ILiturgieOplossing LosOp(ILiturgieInterpretatie item)
        {
            var regel = new Regel();

            // verwerk de opties
            var trimmedOpties = item.Opties.Select(o => o.Trim()).ToList();
            regel.VerwerkenAlsSlide = !trimmedOpties.Any(o => o.StartsWith(LiturgieDatabaseSettings.OptieNietVerwerken, StringComparison.CurrentCultureIgnoreCase));
            regel.TonenInVolgende = !trimmedOpties.Any(o => o.StartsWith(LiturgieDatabaseSettings.OptieNietTonenInVolgende, StringComparison.CurrentCultureIgnoreCase));
            regel.TonenInOverzicht = !trimmedOpties.Any(o => o.StartsWith(LiturgieDatabaseSettings.OptieNietTonenInOverzicht, StringComparison.CurrentCultureIgnoreCase));

            // zoek de regels in de database en pak ook de naamgeving daar uit over
            if (regel.VerwerkenAlsSlide)
            {
                var setNaam = item.Benaming;
                var zoekNaam = item.Deel;
                if (string.IsNullOrEmpty(item.Deel))
                {
                    setNaam = FileEngineDefaults.CommonFilesSetName;
                    zoekNaam = item.Benaming;
                    regel.TonenInOverzicht = false;  // TODO tijdelijk default gedrag van het niet tonen van algemene items in het overzicht overgenomen uit de oude situatie
                }
                var fout = Aanvullen(_database, regel, setNaam, zoekNaam, item.Verzen.ToList());
                if (fout.HasValue)
                    return new Oplossing(fout.Value, item);
            }

            // kijk of de opties nog iets zeggen over alternatieve naamgeving
            var optieMetAltNaamOverzicht = GetOptieParam(trimmedOpties, LiturgieDatabaseSettings.OptieAlternatieveNaamOverzicht);
            if (!string.IsNullOrWhiteSpace(optieMetAltNaamOverzicht))
                regel.OverzichtDisplay = optieMetAltNaamOverzicht;
            var optieMetAltNaamVolgende = GetOptieParam(trimmedOpties, LiturgieDatabaseSettings.OptieAlternatieveNaam);
            if (!string.IsNullOrWhiteSpace(optieMetAltNaamVolgende))
                regel.NaamDisplay = optieMetAltNaamOverzicht;

            // geef de oplossing terug
            return new Oplossing(LiturgieOplossingResultaat.Opgelost, item, regel);
        }

        private static LiturgieOplossingResultaat? Aanvullen(IEngine<FileEngineSetSettings> db, Regel regel, string setNaam, string zoekNaam, IEnumerable<string> verzen)
        {
            // zoek de set via op-schijf naam of de aangepaste naam
            var set = db.Where(s => string.Compare(s.Name, setNaam, true) == 0 || string.Compare(s.Settings.DisplayName, setNaam, true) == 0).FirstOrDefault();
            if (set == null)
                return LiturgieOplossingResultaat.SetFout;
            // Je kunt geen verzen opgeven als we ze niet los hebben.
            // (Andere kant op kan wel: geen verzen opgeven terwijl ze er wel zijn (wat 'alle verzen' betekend))
            if (verzen.Any() && !set.Settings.ItemsHaveSubContent)
                return LiturgieOplossingResultaat.VersOnderverdelingMismatch;
            // Kijk of we het specifieke item in de set kunnen vinden (alleen via de op-schijf naam)
            var subSet = set.Where(r => string.Compare(r.Name, zoekNaam, true) == 0).FirstOrDefault();
            if (subSet == null)
                return LiturgieOplossingResultaat.SubSetFout;
            if (setNaam == FileEngineDefaults.CommonFilesSetName)
                regel.NaamDisplay = subSet.Name;
            else
                regel.SubNaamDisplay = subSet.Name;
            if (!verzen.Any())
            {
                // Als de set zonder verzen is hebben we n samengevoegd item
                if (!set.Settings.ItemsHaveSubContent)
                {
                    var content = KrijgDirecteContent(subSet.Content, null);
                    if (content == null)
                        return LiturgieOplossingResultaat.VersOnleesbaar;
                    regel.Content = new List<ILiturgieContent>() { content };
                }
                // Een item met alle verzen
                else
                {
                    regel.Content = subSet.Content.TryAccessSubs()
                        .Select(s => KrijgDirecteContent(s.Content, s.Name))
                        .Where(s => s != null)  // omdat we alles ophalen kunnen hier dingen in zitten die niet kloppen
                        .OrderBy(s => s.Nummer)  // Op volgorde van nummer
                        .ToList();
                }
            }
            else
            {
                // Specifieke verzen
                var content = subSet.Content.TryAccessSubs();
                var preSelect = InterpreteerNummers(verzen)  // We houden de volgorde van het opgeven aan omdat die afwijkend kan zijn
                    .Select(n => n.ToString())
                    .Select(n => new { Naam = n, SubSet = content.FirstOrDefault(c => c.Name == n), })
                    .ToList();
                // Specifieke verzen moeten allemaal gevonden kunnen worden
                if (preSelect.Any(c => c.SubSet == null))
                    return LiturgieOplossingResultaat.VersFout;
                regel.Content = preSelect
                    .Select(s => KrijgDirecteContent(s.SubSet.Content, s.Naam))
                    .ToList();
                // Specifieke verzen moeten allemaal interpreteerbaar zijn
                if (regel.Content.Any(c => c == null))
                    return LiturgieOplossingResultaat.VersOnleesbaar;
            }

            // bepaal de naamgeving
            regel.NaamDisplay = !string.IsNullOrWhiteSpace(set.Settings.DisplayName) ? set.Settings.DisplayName : regel.NaamDisplay;
            regel.OverzichtDisplay = regel.NaamDisplay;

            return null;
        }

        private static Content KrijgDirecteContent(IDbItemContent metItem, string possibleNummer)
        {
            int? nummer = null;
            int parseNummer;
            if (int.TryParse(possibleNummer ?? "", out parseNummer))
                nummer = parseNummer;
            if (metItem.Type == "txt")
            {
                using (var fs = metItem.Content)
                {
                    using (var rdr = new StreamReader(fs))
                    {
                        // geef de inhoud als tekst terug
                        return new Content() { Inhoud = rdr.ReadToEnd(), InhoudType = InhoudType.Tekst, Nummer = nummer };
                    }
                }
            }
            else if (metItem.Type == "ppt" || metItem.Type == "pptx")
            {
                // geef de inhoud als link terug
                return new Content() { Inhoud = metItem.PersistentLink, InhoudType = InhoudType.PptLink, Nummer = nummer };
            }
            // Geef een leeg item terug als we het niet konden verwerken
            return null;
        }


        private static string GetOptieParam(IEnumerable<string> opties, string optie)
        {
            var optieMetParam = opties.FirstOrDefault(o => o.StartsWith(optie, StringComparison.CurrentCultureIgnoreCase));
            if (optieMetParam == null)
                return null;
            return optieMetParam.Substring(optie.Length).Trim();
        }

        private static IEnumerable<int> InterpreteerNummers(IEnumerable<string> nummers)
        {
            foreach (var nummer in nummers)
            {
                var safeNummer = (nummer ?? "").Trim();
                int parseNummer;
                if (int.TryParse(safeNummer, out parseNummer)) {
                    yield return parseNummer;
                    continue;
                }
                if (safeNummer.Contains(LiturgieDatabaseSettings.VersSamenvoeging))
                {
                    var split = safeNummer.Split(new string[] { LiturgieDatabaseSettings.VersSamenvoeging }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length != 2)
                    {
                        int vanNummer;
                        int totNummer;
                        if (int.TryParse(split[0].Trim(), out vanNummer) && int.TryParse(split[1].Trim(), out totNummer))
                        {
                            foreach (var teller in Enumerable.Range(vanNummer, totNummer - vanNummer))
                                yield return teller;
                        }
                    }
                }
                // TODO fout, hoe naar buiten laten komen?
            }
        }


        public IEnumerable<ILiturgieOplossing> LosOp(IEnumerable<ILiturgieInterpretatie> items)
        {
            return items.Select(LosOp).ToList();
        }



        private class Oplossing : ILiturgieOplossing
        {
            public LiturgieOplossingResultaat Resultaat { get; set; }
            public ILiturgieInterpretatie VanInterpretatie { get; set; }
            public ILiturgieRegel Regel { get; set; }

            public Oplossing(LiturgieOplossingResultaat resultaat, ILiturgieInterpretatie interpretatie, ILiturgieRegel regel = null)
            {
                Resultaat = resultaat;
                VanInterpretatie = interpretatie;
                Regel = regel;
            }
        }
        private class Regel : ILiturgieRegel
        {
            public string NaamDisplay { get; set; }
            public string OverzichtDisplay { get; set; }
            public string SubNaamDisplay { get; set; }
            public IEnumerable<ILiturgieContent> Content { get; set; }
            public bool TonenInOverzicht { get; set; }
            public bool TonenInVolgende { get; set; }
            public bool VerwerkenAlsSlide { get; set; }
        }
        private class Content : ILiturgieContent
        {
            public string Inhoud { get; set; }

            public InhoudType InhoudType { get; set; }

            public int? Nummer { get; set; }
        }
    }
}
