using IDatabase;
using ILiturgieDatabase;
using ISettings;
using PowerpointGenerator.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.String;

namespace PowerpointGenerator.Database {

    // TODO Stap 1: assistentie bij invullen liturgie
    // TODO mask weer ergens toepassen



    /// <summary>
    /// Maak een ruwe lijst van een tekstuele liturgie
    /// </summary>
    public class InterpreteerLiturgieRuw : ILiturgieInterpreteer
    {
        private static readonly char[] BenamingScheidingstekens = { ':' };
        private static readonly char[] BenamingDeelScheidingstekens = { ' ' };
        private static readonly char[] VersScheidingstekens = { ',' };
        private static readonly char[] OptieStart = { '(' };
        private static readonly char[] OptieEinde = { ')' };
        private static readonly char[] OptieScheidingstekens = { ',' };

        private static Interpretatie SplitTekstregel(string invoer)
        {
            var regel = new Interpretatie();
            var invoerTrimmed = invoer.Trim();
            var voorOpties = invoerTrimmed.Split(OptieStart, StringSplitOptions.RemoveEmptyEntries);
            if (voorOpties.Length == 0)
                return null;
            var opties = voorOpties.Length > 1 ? voorOpties[1].Split(OptieEinde, StringSplitOptions.RemoveEmptyEntries)[0].Trim() : Empty;
            var voorBenamingStukken = voorOpties[0].Trim().Split(BenamingScheidingstekens, StringSplitOptions.RemoveEmptyEntries);
            if (voorBenamingStukken.Length == 0)
                return null;
            var preBenamingTrimmed = voorBenamingStukken[0].Trim();
            // Een benaming kan uit delen bestaan, bijvoorbeeld 'psalm 110' in 'psalm 110:1,2' of 'opwekking 598' in 'opwekking 598'
            var voorPreBenamingStukken = preBenamingTrimmed.Split(BenamingDeelScheidingstekens, StringSplitOptions.RemoveEmptyEntries);
            if (voorPreBenamingStukken.Length > 1)
                regel.Deel = voorPreBenamingStukken[voorPreBenamingStukken.Length - 1];  // Is altijd laatste deel
            regel.Benaming = preBenamingTrimmed.Substring(0, preBenamingTrimmed.Length - (regel.Deel ?? "").Length).Trim();
            // Verzen als '1,2' in 'psalm 110:1,2'
            regel.VerzenZoalsIngevoerd = voorBenamingStukken.Length > 1 ? voorBenamingStukken[1] : null;
            regel.Verzen = (regel.VerzenZoalsIngevoerd ?? "")
              .Split(VersScheidingstekens, StringSplitOptions.RemoveEmptyEntries)
              .Select(v => v.Trim())
              .ToList();
            regel.Opties = (!IsNullOrEmpty(opties) ? opties : "")
              .Split(OptieScheidingstekens, StringSplitOptions.RemoveEmptyEntries)
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
              .Where(r => !IsNullOrWhiteSpace(r))
              .Select(VanTekstregel)
              .Where(r => r != null)
              .ToList();
        }

        private class Interpretatie : ILiturgieInterpretatie
        {
            public string Benaming { get; set; }
            public string Deel { get; set; }

            public IEnumerable<string> Opties { get; set; }
            public IEnumerable<string> Verzen { get; set; }
            public string VerzenZoalsIngevoerd { get; set; }

            public override string ToString()
            {
                return $"{Benaming} {Deel} {VerzenZoalsIngevoerd}";
            }
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
    public class LiturgieDatabase : ILiturgieLosOp
    {
        private readonly IEngine<FileEngineSetSettings> _database;
        public LiturgieDatabase(IEngine<FileEngineSetSettings> database)
        {
            _database = database;
        }


        public ILiturgieOplossing LosOp(ILiturgieInterpretatie item)
        {
            return LosOp(item, null);
        }
        public ILiturgieOplossing LosOp(ILiturgieInterpretatie item, IEnumerable<ILiturgieMapmaskArg> masks)
        {
            var regel = new Regel {DisplayEdit = new RegelDisplay()};

            // verwerk de opties
            var trimmedOpties = item.Opties.Select(o => o.Trim()).ToList();
            regel.VerwerkenAlsSlide = !trimmedOpties.Any(o => o.StartsWith(LiturgieDatabaseSettings.OptieNietVerwerken, StringComparison.CurrentCultureIgnoreCase));
            regel.TonenInVolgende = !trimmedOpties.Any(o => o.StartsWith(LiturgieDatabaseSettings.OptieNietTonenInVolgende, StringComparison.CurrentCultureIgnoreCase));
            regel.TonenInOverzicht = !trimmedOpties.Any(o => o.StartsWith(LiturgieDatabaseSettings.OptieNietTonenInOverzicht, StringComparison.CurrentCultureIgnoreCase));

            // regel visualisatie default
            regel.DisplayEdit.Naam = item.Benaming;
            regel.DisplayEdit.SubNaam = item.Deel;
            regel.DisplayEdit.VersenGebruikDefault = new VersenDefault();

            // zoek de regels in de database en pak ook de naamgeving daar uit over
            if (regel.VerwerkenAlsSlide)
            {
                var setNaam = item.Benaming;
                var zoekNaam = item.Deel;
                if (IsNullOrEmpty(item.Deel))
                {
                    setNaam = FileEngineDefaults.CommonFilesSetName;
                    zoekNaam = item.Benaming;
                    regel.TonenInOverzicht = false;  // TODO tijdelijk default gedrag van het niet tonen van algemene items in het overzicht overgenomen uit de oude situatie
                }
                
                var fout = Aanvullen(_database, regel, setNaam, zoekNaam, item.Verzen.ToList());
                if (fout.HasValue)
                    return new Oplossing(fout.Value, item);
            } else
            {
                regel.DisplayEdit.VersenGebruikDefault = new VersenDefault(item.VerzenZoalsIngevoerd);
            }

            // Underscores als spaties tonen
            regel.DisplayEdit.Naam = (regel.DisplayEdit.Naam ?? "").Replace("_", " ");
            regel.DisplayEdit.SubNaam = (regel.DisplayEdit.SubNaam ?? "").Replace("_", " ");
            // Check of er een mask is (mooiere naam)
            var maskCheck = masks?.FirstOrDefault(m => Compare(m.RealName, regel.DisplayEdit.Naam, true) == 0);
            if (maskCheck != null)
                regel.DisplayEdit.Naam = maskCheck.Name;
            // Check of de hoofdnaam genegeerd moet worden (is leeg)
            if (IsNullOrWhiteSpace(regel.DisplayEdit.Naam) && !IsNullOrWhiteSpace(regel.DisplayEdit.SubNaam))
            {
                regel.DisplayEdit.Naam = regel.Display.SubNaam;
                regel.DisplayEdit.SubNaam = null;
            }
            // regel visualisatie na bewerking
            if (IsNullOrEmpty(regel.DisplayEdit.NaamOverzicht))
                regel.DisplayEdit.NaamOverzicht = regel.DisplayEdit.Naam;
            // kijk of de opties nog iets zeggen over alternatieve naamgeving
            var optieMetAltNaamOverzicht = GetOptieParam(trimmedOpties, LiturgieDatabaseSettings.OptieAlternatieveNaamOverzicht);
            if (!IsNullOrWhiteSpace(optieMetAltNaamOverzicht))
                regel.DisplayEdit.NaamOverzicht = optieMetAltNaamOverzicht;
            var optieMetAltNaamVolgende = GetOptieParam(trimmedOpties, LiturgieDatabaseSettings.OptieAlternatieveNaam);
            if (!IsNullOrWhiteSpace(optieMetAltNaamVolgende))
                regel.DisplayEdit.Naam = optieMetAltNaamOverzicht;

            // geef de oplossing terug
            return new Oplossing(LiturgieOplossingResultaat.Opgelost, item, regel);
        }

        private static LiturgieOplossingResultaat? Aanvullen(IEngine<FileEngineSetSettings> db, Regel regel, string setNaam, string zoekNaam, IEnumerable<string> verzen)
        {
            var verzenList = verzen.ToList();
            // zoek de set via op-schijf naam of de aangepaste naam
            var set = db.Where(s => Compare(s.Name, setNaam, StringComparison.OrdinalIgnoreCase) == 0 || Compare(s.Settings.DisplayName, setNaam, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
            if (set == null)
                return LiturgieOplossingResultaat.SetFout;
            // Je kunt geen verzen opgeven als we ze niet los hebben.
            // (Andere kant op kan wel: geen verzen opgeven terwijl ze er wel zijn (wat 'alle verzen' betekend))
            if (verzenList.Any() && !set.Settings.ItemsHaveSubContent)
                return LiturgieOplossingResultaat.VersOnderverdelingMismatch;
            // Kijk of we het specifieke item in de set kunnen vinden (alleen via de op-schijf naam)
            var subSet = set.Where(r => Compare(r.Name, zoekNaam, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
            if (subSet == null)
                return LiturgieOplossingResultaat.SubSetFout;
            if (setNaam == FileEngineDefaults.CommonFilesSetName)
            {
                regel.DisplayEdit.Naam = subSet.Name;
                regel.DisplayEdit.VersenGebruikDefault = new VersenDefault(string.Empty);  // Expliciet: Common bestanden hebben nooit versen
            }
            else {
                regel.DisplayEdit.Naam = set.Name;
                regel.DisplayEdit.SubNaam = subSet.Name;
            }
            if (!verzenList.Any())
            {
                // We hebben geen versenlijst en de set instellingen zeggen zonder verzen te zijn dus hebben we n samengevoegd item
                if (!set.Settings.ItemsHaveSubContent)
                {
                    var content = KrijgDirecteContent(subSet.Content, null);
                    if (content == null)
                        return LiturgieOplossingResultaat.VersOnleesbaar;
                    regel.Content = new List<ILiturgieContent> { content };
                    regel.DisplayEdit.VersenGebruikDefault = new VersenDefault(string.Empty);  // Altijd default gebruiken omdat er altijd maar 1 content is
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
                regel.DisplayEdit.VolledigeContent = true;
            }
            else
            {
                // Specifieke verzen
                var content = subSet.Content.TryAccessSubs();
                var preSelect = InterpreteerNummers(verzenList)  // We houden de volgorde van het opgeven aan omdat die afwijkend kan zijn
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
                regel.DisplayEdit.VolledigeContent = false;
            }

            // bepaal de naamgeving
            regel.DisplayEdit.Naam = !IsNullOrWhiteSpace(set.Settings.DisplayName) ? (set.Settings.DisplayName.Equals(Properties.Settings.Default.SetNameEmpty, StringComparison.CurrentCultureIgnoreCase) ? null : set.Settings.DisplayName) : regel.DisplayEdit.Naam;

            return null;
        }

        private static Content KrijgDirecteContent(IDbItemContent metItem, string possibleNummer)
        {
            int? nummer = null;
            int parseNummer;
            if (int.TryParse(possibleNummer ?? "", out parseNummer))
                nummer = parseNummer;
            switch (metItem.Type)
            {
                case "txt":
                    using (var fs = metItem.Content)
                    {
                        using (var rdr = new StreamReader(fs))
                        {
                            // geef de inhoud als tekst terug
                            return new Content { Inhoud = rdr.ReadToEnd(), InhoudType = InhoudType.Tekst, Nummer = nummer };
                        }
                    }
                case "ppt":
                case "pptx":
                    // geef de inhoud als link terug
                    return new Content { Inhoud = metItem.PersistentLink, InhoudType = InhoudType.PptLink, Nummer = nummer };
            }
            // Geef een leeg item terug als we het niet konden verwerken
            return null;
        }


        private static string GetOptieParam(IEnumerable<string> opties, string optie)
        {
            var optieMetParam = opties.FirstOrDefault(o => o.StartsWith(optie, StringComparison.CurrentCultureIgnoreCase));
            return optieMetParam?.Substring(optie.Length).Trim();
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
                    var split = safeNummer.Split(new[] { LiturgieDatabaseSettings.VersSamenvoeging }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length == 2)
                    {
                        int vanNummer;
                        int totEnMetNummer;
                        if (int.TryParse(split[0].Trim(), out vanNummer) && int.TryParse(split[1].Trim(), out totEnMetNummer))
                        {
                            foreach (var teller in Enumerable.Range(vanNummer, totEnMetNummer - vanNummer + 1))
                                yield return teller;
                        }
                    }
                }
                // TODO fout, hoe naar buiten laten komen?
            }
        }


        public IEnumerable<ILiturgieOplossing> LosOp(IEnumerable<ILiturgieInterpretatie> items, IEnumerable<ILiturgieMapmaskArg> masks)
        {
            return items.Select(i => LosOp(i, masks)).ToList();
        }



        private class Oplossing : ILiturgieOplossing
        {
            public LiturgieOplossingResultaat Resultaat { get; }
            public ILiturgieInterpretatie VanInterpretatie { get; }
            public ILiturgieRegel Regel { get; }

            public Oplossing(LiturgieOplossingResultaat resultaat, ILiturgieInterpretatie interpretatie, ILiturgieRegel regel = null)
            {
                Resultaat = resultaat;
                VanInterpretatie = interpretatie;
                Regel = regel;
            }
        }
        private class Regel : ILiturgieRegel
        {
            public ILiturgieDisplay Display => DisplayEdit;
            public RegelDisplay DisplayEdit;

            public IEnumerable<ILiturgieContent> Content { get; set; }

            public bool TonenInOverzicht { get; set; }
            public bool TonenInVolgende { get; set; }
            public bool VerwerkenAlsSlide { get; set; }

            public override string ToString()
            {
                return $"{Display.Naam} {Display.SubNaam}";
            }
        }
        private class RegelDisplay : ILiturgieDisplay
        {
            public string Naam { get; set; }
            public string NaamOverzicht { get; set; }
            public string SubNaam { get; set; }
            public bool VolledigeContent { get; set; }
            public IVersenDefault VersenGebruikDefault { get; set; }
        }
        private class VersenDefault : IVersenDefault
        {
            public VersenDefault()
            {
                Gebruik = false;
            }
            public VersenDefault(string tekst)
            {
                Tekst = tekst;
                Gebruik = true;
            }

            public bool Gebruik { get; set; }
            public string Tekst { get; set; }
        }

        private class Content : ILiturgieContent
        {
            public string Inhoud { get; set; }

            public InhoudType InhoudType { get; set; }

            public int? Nummer { get; set; }
        }
    }

    static class MapMasksToLiturgie
    {
        public static IEnumerable<ILiturgieMapmaskArg> Map(IEnumerable<IMapmask> masks)
        {
            return masks.Select(m => new MaskMap { Name = m.Name, RealName = m.RealName }).ToList();
        }

        private class MaskMap : ILiturgieMapmaskArg
        {
            public string Name { get; set; }

            public string RealName { get; set; }
        }
    }

}
