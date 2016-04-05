﻿// Copyright 2016 door Erik de Roos
using Generator.Database.FileSystem;
using IDatabase;
using ILiturgieDatabase;
using ISettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static System.String;

namespace Generator.Database
{

    // TODO Stap 1: assistentie bij invullen liturgie

    public static class LiturgieDatabaseSettings
    {
        public const string VersSamenvoeging = "-";
        public const string DatabaseNameBijbeltekst = "bijbel";
    }

    /// <summary>
    /// Zoek naar de opgegeven liturgieen.
    /// </summary>
    public class LiturgieDatabase : ILiturgieDatabase.ILiturgieDatabase
    {
        private readonly IEngineManager<FileEngineSetSettings> _databases;
        public LiturgieDatabase(IEngineManager<FileEngineSetSettings> database)
        {
            _databases = database;
        }

        public IZoekresultaat ZoekOnderdeel(string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen = null)
        {
            return ZoekOnderdeel(VerwerkingType.normaal, onderdeelNaam, fragmentNaam, fragmentDelen);
        }

        public IZoekresultaat ZoekOnderdeel(VerwerkingType alsType, string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen = null)
        {
            // TODO alsType voor bijbelteksten bij nummering ondersteuning voor verzen tot einde hoofdstukken bijv '20 -'
            // TODO alsType voor bijbelteksten in content referentie naar hoofdstuk (of op een andere plaats) zodat hoofdstuk wisseling bepaald kan worden

            var database = alsType == VerwerkingType.normaal ? _databases.GetDefault() : _databases.Extensions.FirstOrDefault(e => e.Name == LiturgieDatabaseSettings.DatabaseNameBijbeltekst);
            if (database == null)
                return new Zoekresultaat() { Status = LiturgieOplossingResultaat.DatabaseFout };

            var set = database.Engine.Where(s => Compare(s.Name, onderdeelNaam, StringComparison.OrdinalIgnoreCase) == 0 || Compare(s.Settings.DisplayName, onderdeelNaam, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
            if (set == null)
                return new Zoekresultaat() { Status = LiturgieOplossingResultaat.SetFout };
            // Kijk of we het specifieke item in de set kunnen vinden (alleen via de op-schijf naam)
            var subSet = set.Where(r => Compare(r.Name, fragmentNaam, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
            if (subSet == null)
                return new Zoekresultaat() { Status = LiturgieOplossingResultaat.SubSetFout };
            var returnValue = new Zoekresultaat()
            {
                OnderdeelNaam = set.Name,
                FragmentNaam = subSet.Name,
                ZonderContentSplitsing = !set.Settings.ItemsHaveSubContent,
                OnderdeelDisplayNaam = set.Settings.DisplayName,
            };
            // Je kunt geen verzen opgeven als we ze niet los hebben.
            // (Andere kant op kan wel: geen verzen opgeven terwijl ze er wel zijn (wat 'alle verzen' betekend))
            if (fragmentDelen != null && fragmentDelen.Any() && !(set.Settings.ItemsHaveSubContent || set.Settings.ItemIsSubContent))
                return new Zoekresultaat() { Status = LiturgieOplossingResultaat.VersOnderverdelingMismatch };
            if (fragmentDelen == null || !fragmentDelen.Any())
            {
                // We hebben geen versenlijst en de set instellingen zeggen zonder verzen te zijn dus hebben we n samengevoegd item
                if (!(set.Settings.ItemsHaveSubContent || set.Settings.ItemIsSubContent))
                {
                    var content = KrijgDirecteContent(subSet.Content, null);
                    if (content == null)
                        return new Zoekresultaat() { Status = LiturgieOplossingResultaat.VersOnleesbaar };
                    returnValue.Content = new List<ILiturgieContent> { content };
                }
                // Een item met alle verzen
                else
                {
                    returnValue.Content = KrijgContentDelayed(subSet, set.Settings)
                        .Select(s => s.GetContent())
                        .Where(s => s != null)  // omdat we alles ophalen kunnen hier dingen in zitten die niet kloppen
                        .OrderBy(s => s.Nummer)  // Op volgorde van nummer
                        .ToList();
                }
            }
            else
            {
                // Specifieke verzen
                var delayedContent = KrijgContentDelayed(subSet, set.Settings);
                var preSelect = InterpreteerNummers(fragmentDelen)  // We houden de volgorde van het opgeven aan omdat die afwijkend kan zijn
                    .Select(n => n.ToString())
                    .Select(n => new { Naam = n, SubSet = delayedContent.FirstOrDefault(c => c.Name == n), })
                    .ToList();
                // Specifieke verzen moeten allemaal gevonden kunnen worden
                if (preSelect.Any(c => c.SubSet == null))
                    return new Zoekresultaat() { Status = LiturgieOplossingResultaat.VersFout };
                returnValue.Content = preSelect
                    .Select(s => s.SubSet.GetContent())
                    .ToList();
                // Specifieke verzen moeten allemaal interpreteerbaar zijn
                if (returnValue.Content.Any(c => c == null))
                    return new Zoekresultaat() { Status = LiturgieOplossingResultaat.VersOnleesbaar };
            }
            returnValue.Status = LiturgieOplossingResultaat.Opgelost;
            return returnValue;
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

        private static IEnumerable<IContentDelayed> KrijgContentDelayed(IDbItem vanItem, FileEngineSetSettings metSettings)
        {
            if (metSettings.ItemsHaveSubContent)
                return vanItem.Content.TryAccessSubs()
                    .Select(s => new ContentDelayed(s.Name, s.Content))
                    .ToList();
            if (metSettings.ItemIsSubContent) {
                var content = KrijgDirecteContent(vanItem.Content, null);
                if (content.InhoudType == InhoudType.Tekst)
                    return SplitFile(content.Inhoud)
                        .Select(s => new ContentDirect(s))
                        .ToList();
            }
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

        private static IEnumerable<Content> SplitFile(string fileContent)
        {
            int bezigMetNummer = 1;
            int positieInFileContent = 0;
            var nummerBuilder = new StringBuilder();
            var nummerFindRegex = new System.Text.RegularExpressions.Regex(@"(^|\s)(\d+)($|\s)", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.Compiled);
            while (positieInFileContent < fileContent.Length)
            {
                var volgendeNummerMatch = nummerFindRegex.Match(fileContent, positieInFileContent);
                if (!volgendeNummerMatch.Success)
                {
                    nummerBuilder.Append(fileContent, positieInFileContent, fileContent.Length - positieInFileContent);
                    yield return new Content() { Inhoud = nummerBuilder.ToString().Trim(' '), InhoudType = InhoudType.Tekst, Nummer = bezigMetNummer };
                    break;
                }

                var volgendeNummerMatchGroep = volgendeNummerMatch.Groups[2];
                nummerBuilder.Append(fileContent, positieInFileContent, volgendeNummerMatchGroep.Index - positieInFileContent);
                positieInFileContent = volgendeNummerMatchGroep.Index + volgendeNummerMatchGroep.Length;

                var volgendNummer = int.Parse(volgendeNummerMatchGroep.Value);
                if (volgendNummer == bezigMetNummer + 1)
                {
                    yield return new Content() { Inhoud = nummerBuilder.ToString().Trim(' '), InhoudType = InhoudType.Tekst, Nummer = bezigMetNummer };
                    nummerBuilder = new StringBuilder();
                    bezigMetNummer++;
                }
            }
        }

        private interface IContentDelayed
        {
            string Name { get; }
            Content GetContent();
        }
        private class ContentDelayed : IContentDelayed
        {
            public string Name { get; private set; }
            private IDbItemContent _dbItem;
            public ContentDelayed(string name, IDbItemContent dbItem)
            {
                Name = name;
                _dbItem = dbItem;
            }
            public Content GetContent()
            {
                return KrijgDirecteContent(_dbItem, Name);
            }
        }
        private class ContentDirect : IContentDelayed
        {
            public string Name { get; private set; }
            private Content _content;
            public ContentDirect(Content content)
            {
                Name = content.Nummer.ToString();
                _content = content;
            }
            public Content GetContent()
            {
                return _content;
            }
        }

        private class Content : ILiturgieContent
        {
            public string Inhoud { get; set; }

            public InhoudType InhoudType { get; set; }

            public int? Nummer { get; set; }
        }

        public class Zoekresultaat : IZoekresultaat
        {
            public LiturgieOplossingResultaat Status { get; set; }
            public string OnderdeelNaam { get; set; }
            public string OnderdeelDisplayNaam { get; set; }
            public string FragmentNaam { get; set; }
            public IEnumerable<ILiturgieContent> Content { get; set; }
            public bool ZonderContentSplitsing { get; set; }
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
