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
            regel.VerzenZoalsIngevoerd = voorBenamingStukken.Length > 1 ? voorBenamingStukken[1].Trim() : null;
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

    public interface ILiturgieDatabase
    {
        IZoekresultaat ZoekOnderdeel(string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen = null);
    }
    public interface IZoekresultaat
    {
        LiturgieOplossingResultaat? Fout { get; }
        string OnderdeelNaam { get; }
        string OnderdeelDisplayNaam { get; }
        string FragmentNaam { get; }
        IEnumerable<ILiturgieContent> Content { get; }
        bool ZonderContentSplitsing { get; }
    }


    /// <summary>
    /// Zoek naar de opgegeven liturgieen.
    /// </summary>
    public class LiturgieDatabase : ILiturgieDatabase
    {
        private readonly IEngine<FileEngineSetSettings> _database;
        public LiturgieDatabase(IEngine<FileEngineSetSettings> database)
        {
            _database = database;
        }

        public IZoekresultaat ZoekOnderdeel(string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen = null)
        {
            var set = _database.Where(s => Compare(s.Name, onderdeelNaam, StringComparison.OrdinalIgnoreCase) == 0 || Compare(s.Settings.DisplayName, onderdeelNaam, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
            if (set == null)
                return new Zoekresultaat() { Fout = LiturgieOplossingResultaat.SetFout };
            // Je kunt geen verzen opgeven als we ze niet los hebben.
            // (Andere kant op kan wel: geen verzen opgeven terwijl ze er wel zijn (wat 'alle verzen' betekend))
            if (fragmentDelen != null && fragmentDelen.Any() && !set.Settings.ItemsHaveSubContent)
                return new Zoekresultaat() { Fout = LiturgieOplossingResultaat.VersOnderverdelingMismatch };
            // Kijk of we het specifieke item in de set kunnen vinden (alleen via de op-schijf naam)
            var subSet = set.Where(r => Compare(r.Name, fragmentNaam, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
            if (subSet == null)
                return new Zoekresultaat() { Fout = LiturgieOplossingResultaat.SubSetFout };
            var returnValue = new Zoekresultaat()
            {
                OnderdeelNaam = set.Name,
                FragmentNaam = subSet.Name,
                ZonderContentSplitsing = !set.Settings.ItemsHaveSubContent,
                OnderdeelDisplayNaam = set.Settings.DisplayName,
            };
            if (fragmentDelen == null || !fragmentDelen.Any())
            {
                // We hebben geen versenlijst en de set instellingen zeggen zonder verzen te zijn dus hebben we n samengevoegd item
                if (!set.Settings.ItemsHaveSubContent)
                {
                    var content = KrijgDirecteContent(subSet.Content, null);
                    if (content == null)
                        return new Zoekresultaat() { Fout = LiturgieOplossingResultaat.VersOnleesbaar };
                    returnValue.Content = new List<ILiturgieContent> { content };
                }
                // Een item met alle verzen
                else
                {
                    returnValue.Content = subSet.Content.TryAccessSubs()
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
                var preSelect = InterpreteerNummers(fragmentDelen)  // We houden de volgorde van het opgeven aan omdat die afwijkend kan zijn
                    .Select(n => n.ToString())
                    .Select(n => new { Naam = n, SubSet = content.FirstOrDefault(c => c.Name == n), })
                    .ToList();
                // Specifieke verzen moeten allemaal gevonden kunnen worden
                if (preSelect.Any(c => c.SubSet == null))
                    return new Zoekresultaat() { Fout = LiturgieOplossingResultaat.VersFout };
                returnValue.Content = preSelect
                    .Select(s => KrijgDirecteContent(s.SubSet.Content, s.Naam))
                    .ToList();
                // Specifieke verzen moeten allemaal interpreteerbaar zijn
                if (returnValue.Content.Any(c => c == null))
                    return new Zoekresultaat() { Fout = LiturgieOplossingResultaat.VersOnleesbaar };
            }
            return returnValue;
        }

        public class Zoekresultaat : IZoekresultaat
        {
            public LiturgieOplossingResultaat? Fout { get; set; }
            public string OnderdeelNaam { get; set; }
            public string OnderdeelDisplayNaam { get; set; }
            public string FragmentNaam { get; set; }
            public IEnumerable<ILiturgieContent> Content { get; set; }
            public bool ZonderContentSplitsing { get; set; }
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

        private static IEnumerable<int> InterpreteerNummers(IEnumerable<string> nummers)
        {
            foreach (var nummer in nummers)
            {
                var safeNummer = (nummer ?? "").Trim();
                int parseNummer;
                if (int.TryParse(safeNummer, out parseNummer))
                {
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
