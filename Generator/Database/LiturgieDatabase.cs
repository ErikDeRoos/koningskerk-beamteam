// Copyright 2016 door Erik de Roos
using Generator.Database.FileSystem;
using IDatabase;
using ILiturgieDatabase;
using ISettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.String;

namespace Generator.Database
{

    // TODO Stap 1: assistentie bij invullen liturgie
    // TODO mask weer ergens toepassen

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
    public class LiturgieDatabase : ILiturgieDatabase.ILiturgieDatabase
    {
        private readonly IEngine<FileEngineSetSettings> _database;
        public LiturgieDatabase(IEngine<FileEngineSetSettings> database)
        {
            _database = database;
        }

        public IZoekresultaat ZoekOnderdeel(string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen = null)
        {
            return ZoekOnderdeel(VerwerkingType.normaal, onderdeelNaam, fragmentNaam, fragmentDelen);
        }

        public IZoekresultaat ZoekOnderdeel(VerwerkingType alsType, string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen = null)
        {
            // TODO alsType voor bijbelteksten uit eigen dir halen
            // TODO alsType voor bijbelteksten bij nummering ondersteuning voor verzen tot einde hoofdstukken bijv '20 -'
            // TODO alsType voor bijbelteksten in content referentie naar hoofdstuk (of op een andere plaats) zodat hoofdstuk wisseling bepaald kan worden

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
