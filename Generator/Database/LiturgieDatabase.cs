// Copyright 2017 door Erik de Roos
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

    public static class LiturgieDatabaseSettings
    {
        public const string VersSamenvoeging = "-";
        public const string DatabaseNameDefault = "default";
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

        public IOplossing ZoekOnderdeel(string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen = null)
        {
            return ZoekOnderdeel(VerwerkingType.normaal, onderdeelNaam, fragmentNaam, fragmentDelen);
        }

        public IOplossing ZoekOnderdeel(VerwerkingType alsType, string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen = null)
        {
            var database = alsType == VerwerkingType.normaal ? _databases.GetDefault() : _databases.Extensions.FirstOrDefault(e => e.Name == LiturgieDatabaseSettings.DatabaseNameBijbeltekst);
            if (database == null)
                return new Oplossing() { Status = LiturgieOplossingResultaat.DatabaseFout };

            var set = database.Engine.Where(s => string.Equals(s.Name, onderdeelNaam, StringComparison.OrdinalIgnoreCase) || string.Equals(s.Settings.DisplayName, onderdeelNaam, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (set == null)
                return new Oplossing() { Status = LiturgieOplossingResultaat.SetFout };
            // Kijk of we het specifieke item in de set kunnen vinden (alleen via de op-schijf naam)
            var subSet = set.Where(r => Compare(r.Name, fragmentNaam, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
            if (subSet == null)
                return new Oplossing() { Status = LiturgieOplossingResultaat.SubSetFout };
            var returnValue = new Oplossing()
            {
                OnderdeelNaam = set.Name,
                FragmentNaam = subSet.Name,
                ZonderContentSplitsing = !set.Settings.ItemsHaveSubContent,
                OnderdeelDisplayNaam = set.Settings.DisplayName,
            };
            // Je kunt geen verzen opgeven als we ze niet los hebben.
            // (Andere kant op kan wel: geen verzen opgeven terwijl ze er wel zijn (wat 'alle verzen' betekend))
            if (fragmentDelen != null && fragmentDelen.Any() && !(set.Settings.ItemsHaveSubContent || set.Settings.ItemIsSubContent))
                return new Oplossing() { Status = LiturgieOplossingResultaat.VersOnderverdelingMismatch };
            if (fragmentDelen == null || !fragmentDelen.Any())
            {
                // We hebben geen versenlijst en de set instellingen zeggen zonder verzen te zijn dus hebben we n samengevoegd item
                if (!(set.Settings.ItemsHaveSubContent || set.Settings.ItemIsSubContent))
                {
                    var content = KrijgDirecteContent(subSet.Content, null);
                    if (content == null)
                        return new Oplossing() { Status = LiturgieOplossingResultaat.VersOnleesbaar };
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
                var preSelect = InterpreteerNummers(fragmentDelen, delayedContent.Select(c => c.Name))  // We houden de volgorde van het opgeven aan omdat die afwijkend kan zijn
                    .Select(n => n.ToString())
                    .Select(n => new { Naam = n, SubSet = delayedContent.FirstOrDefault(c => c.Name == n), })
                    .ToList();
                // Specifieke verzen moeten allemaal gevonden kunnen worden
                if (preSelect.Any(c => c.SubSet == null))
                    return new Oplossing() { Status = LiturgieOplossingResultaat.VersFout };
                returnValue.Content = preSelect
                    .Select(s => s.SubSet.GetContent())
                    .ToList();
                // Specifieke verzen moeten allemaal interpreteerbaar zijn
                if (returnValue.Content.Any(c => c == null))
                    return new Oplossing() { Status = LiturgieOplossingResultaat.VersOnleesbaar };
            }
            returnValue.Status = LiturgieOplossingResultaat.Opgelost;
            return returnValue;
        }

        private static IEnumerable<int> InterpreteerNummers(IEnumerable<string> nummerSets, IEnumerable<string> avaliableNummers)
        {
            int parseAvaliableNummer = 0;
            var avaliableList = avaliableNummers.Where(s => int.TryParse(s, out parseAvaliableNummer)).Select(s => parseAvaliableNummer).ToList();
            foreach (var nummerSet in nummerSets)
            {
                var safeNummerSet = (nummerSet ?? "").Trim();
                int singleNummer = 0;
                if (int.TryParse(safeNummerSet, out singleNummer))
                {
                    yield return singleNummer;
                    continue;
                }
                if (safeNummerSet.Contains(LiturgieDatabaseSettings.VersSamenvoeging))
                {
                    int vanNummer = 0;
                    int totEnMetNummer = 0;
                    var doRun = false;
                    var split = safeNummerSet.Split(new[] { LiturgieDatabaseSettings.VersSamenvoeging }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length == 2)
                        doRun = int.TryParse(split[0].Trim(), out vanNummer) && int.TryParse(split[1].Trim(), out totEnMetNummer);
                    else
                    {
                        split = safeNummerSet.Split(new[] { LiturgieDatabaseSettings.VersSamenvoeging }, StringSplitOptions.None);
                        if (split.Length == 2)
                        {
                            if (!string.IsNullOrWhiteSpace(split[0]))
                            {
                                doRun = int.TryParse(split[0].Trim(), out vanNummer);
                                totEnMetNummer = avaliableList.Any() ? avaliableList.Max() : 0;
                            }
                            else if (!string.IsNullOrWhiteSpace(split[1]))
                            {
                                doRun = int.TryParse(split[1].Trim(), out totEnMetNummer);
                                vanNummer = 1;
                            }
                        }
                    }
                    if (doRun)
                        foreach (var teller in Enumerable.Range(vanNummer, totEnMetNummer - vanNummer + 1).Where(r => avaliableList.Contains(r)))
                            yield return teller;
                }
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
            return new List<IContentDelayed>();
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
                    using (var fs = metItem.GetContentStream())
                    {
                        var rdr = new StreamReader(fs, Encoding.Default);
                        // geef de inhoud als tekst terug
                        return new Content { Inhoud = rdr.ReadToEnd(), InhoudType = InhoudType.Tekst, Nummer = nummer };
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
            int? skipNaarVolgendNummer = null;
            int positieInFileContent = 0;
            var nummerBuilder = new StringBuilder();
            var nummerFindRegex = new System.Text.RegularExpressions.Regex(@"(^|\s)(\d+)([ -]+\d+)?($|\s)", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.Compiled);
            while (positieInFileContent < fileContent.Length)
            {
                var volgendeNummerMatch = nummerFindRegex.Match(fileContent, positieInFileContent);
                if (!volgendeNummerMatch.Success)
                {
                    nummerBuilder.Append(fileContent, positieInFileContent, fileContent.Length - positieInFileContent);
                    yield return new Content() { Inhoud = nummerBuilder.ToString().Trim(' '), InhoudType = InhoudType.Tekst, Nummer = bezigMetNummer };
                    break;
                }

                if (skipNaarVolgendNummer.HasValue && skipNaarVolgendNummer > bezigMetNummer)
                {
                    bezigMetNummer = skipNaarVolgendNummer.Value;
                    skipNaarVolgendNummer = null;
                }

                var volgendeNummerMatchGroep = volgendeNummerMatch.Groups[2];
                nummerBuilder.Append(fileContent, positieInFileContent, volgendeNummerMatchGroep.Index - positieInFileContent);
                positieInFileContent = volgendeNummerMatchGroep.Index + volgendeNummerMatchGroep.Length;
                var toevoegenAlsGeenMatch = volgendeNummerMatchGroep.Value;

                if (volgendeNummerMatch.Groups[3].Success)
                {
                    var meerdereNummers = volgendeNummerMatch.Groups[3].Value.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    skipNaarVolgendNummer = int.Parse(meerdereNummers[0]);
                    positieInFileContent += volgendeNummerMatch.Groups[3].Length;
                    toevoegenAlsGeenMatch += volgendeNummerMatch.Groups[3].Value;
                }

                var volgendNummer = int.Parse(volgendeNummerMatchGroep.Value);
                if (volgendNummer == bezigMetNummer + 1)
                {
                    yield return new Content() { Inhoud = nummerBuilder.ToString().Trim(' '), InhoudType = InhoudType.Tekst, Nummer = bezigMetNummer };
                    nummerBuilder = new StringBuilder();
                    bezigMetNummer++;
                } else
                if (volgendNummer != bezigMetNummer)
                {
                    nummerBuilder.Append(toevoegenAlsGeenMatch);
                    skipNaarVolgendNummer = null;
                }
            }
        }

        public IEnumerable<IZoekresultaat> KrijgAlleOnderdelen()
        {
            return _databases.Extensions
                .SelectMany(de => de.Engine.GetAllNames().Select(n => new Zoekresultaat() { Database = de.Name, Resultaat = n }));
        }

        public IEnumerable<IZoekresultaat> KrijgOnderdeelDefault()
        {
            return _databases.Extensions.Where(e => e.Name == LiturgieDatabaseSettings.DatabaseNameDefault)
                .SelectMany(de => de.Engine.GetAllNames().Select(n => new Zoekresultaat() { Database = de.Name, Resultaat = n }));
        }

        public IEnumerable<IZoekresultaat> KrijgOnderdeelBijbel()
        {
            return _databases.Extensions.Where(e => e.Name == LiturgieDatabaseSettings.DatabaseNameBijbeltekst)
                .SelectMany(de => de.Engine.GetAllNames().Select(n => new Zoekresultaat() { Database = de.Name, Resultaat = n }));
        }

        public IEnumerable<IZoekresultaat> KrijgAlleFragmenten(string onderdeelNaam)
        {
            return _databases.Extensions.SelectMany(de => 
                de.Engine
                .Where(s => string.Equals(s.Name, onderdeelNaam, StringComparison.OrdinalIgnoreCase) || string.Equals(s.Settings.DisplayName, onderdeelNaam, StringComparison.OrdinalIgnoreCase))
                .SelectMany(set => set.GetAllNames().Select(n => new Zoekresultaat() { Database = de.Name, Resultaat = n }))
            );
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

        public class Oplossing : IOplossing
        {
            public LiturgieOplossingResultaat Status { get; set; }
            public string OnderdeelNaam { get; set; }
            public string OnderdeelDisplayNaam { get; set; }
            public string FragmentNaam { get; set; }
            public IEnumerable<ILiturgieContent> Content { get; set; }
            public bool ZonderContentSplitsing { get; set; }

            public Oplossing()
            {
                Status = LiturgieOplossingResultaat.Onbekend;
            }
        }

        private class Zoekresultaat : IZoekresultaat
        {
            public string Database { get; set; }
            public string Resultaat { get; set; }
        }
    }

    public static class MapMasksToLiturgie
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
