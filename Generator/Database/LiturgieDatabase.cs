// Copyright 2024 door Erik de Roos
using Generator.Database.FileSystem;
using Generator.Database.Models;
using Generator.LiturgieInterpretator.Models;
using ISettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static System.String;

namespace Generator.Database
{
    /// <summary>
    /// Zoek naar de opgegeven liturgieen.
    /// </summary>
    public class LiturgieDatabase : ILiturgieDatabase
    {
        private readonly IEngineManager _databases;

        private readonly Dictionary<IDbItem, IContentDelayed[]> _cachedContentDelayed = new Dictionary<IDbItem, IContentDelayed[]>();
        private readonly Dictionary<IDbItemContent, Content> _cachedContentDirect = new Dictionary<IDbItemContent, Content>();

        public LiturgieDatabase(IEngineManager database)
        {
            _databases = database;
        }

        public IOplossing KrijgItem(VerwerkingType alsType, string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen, LiturgieSettings settings)
        {
            var database = alsType == VerwerkingType.normaal ? _databases.GetDefault() : _databases.Extensions.FirstOrDefault(e => e.Name == LiturgieDatabaseSettings.DatabaseNameBijbeltekst);
            if (database == null)
                return new Oplossing() { Status = DatabaseZoekStatus.DatabaseFout };

            var set = database.Engine.Where(s => string.Equals(s.Name.SafeName, onderdeelNaam, StringComparison.CurrentCultureIgnoreCase) || (settings.GebruikDisplayNameVoorZoeken && string.Equals(s.Settings.DisplayName, onderdeelNaam, StringComparison.CurrentCultureIgnoreCase))).FirstOrDefault();
            if (set == null)
                return new Oplossing() { Status = DatabaseZoekStatus.SetFout };
            // Kijk of we het specifieke item in de set kunnen vinden
            var subSet = set.Where(r => Compare(r.Name.SafeName, fragmentNaam, StringComparison.CurrentCultureIgnoreCase) == 0 || Compare(r.Name.Name, fragmentNaam, StringComparison.CurrentCultureIgnoreCase) == 0).FirstOrDefault();
            if (subSet == null)
                return new Oplossing() { Status = DatabaseZoekStatus.SubSetFout };
            var returnValue = new Oplossing
            {
                Onderdeel = new OplossingOnderdeel
                {
                    Naam = set.Name.Name,
                    VeiligeNaam = set.Name.SafeName,
                    AlternatieveNaam = set.Settings.DisplayName,
                },
                Fragment = new OplossingOnderdeel
                {
                    Naam = subSet.Name.Name,
                    VeiligeNaam = subSet.Name.SafeName,
                },
                ZonderContentSplitsing = !set.Settings.ItemsHaveSubContent,
                StandaardNietTonenInLiturgie = set.Settings.NotVisibleInIndex,
            };
            // Je kunt geen verzen opgeven als we ze niet los hebben.
            // (Andere kant op kan wel: geen verzen opgeven terwijl ze er wel zijn (wat 'alle verzen' betekend))
            if (fragmentDelen != null && fragmentDelen.Any() && !(set.Settings.ItemsHaveSubContent || set.Settings.ItemIsSubContent))
                return new Oplossing() { Status = DatabaseZoekStatus.VersOnderverdelingMismatch };
            if (fragmentDelen == null || !fragmentDelen.Any())
            {
                // We hebben geen versenlijst en de set instellingen zeggen zonder verzen te zijn dus hebben we n samengevoegd item
                if (!(set.Settings.ItemsHaveSubContent || set.Settings.ItemIsSubContent))
                {
                    var content = KrijgDirecteContent(subSet.Content, null);
                    if (content == null)
                        return new Oplossing() { Status = DatabaseZoekStatus.VersOnleesbaar };
                    returnValue.Content = new List<ILiturgieContent> { content };
                }
                // Een item met alle verzen
                else
                {
                    returnValue.Content = KrijgContentDelayed(subSet, set.Settings)
                        .Select(s => s.GetContent())
                        .Where(s => s != null)  // omdat we alles ophalen kunnen hier dingen in zitten die niet kloppen
                        .OrderBy(s => s.Nummer)  // Op volgorde van nummer
                        .ToArray();
                }
            }
            else
            {
                // Specifieke verzen
                var delayedContent = KrijgContentDelayed(subSet, set.Settings);
                var preSelect = InterpreteerNummers(fragmentDelen, delayedContent.Select(c => c.PossibleNummer))  // We houden de volgorde van het opgeven aan omdat die afwijkend kan zijn
                    .Select(n => n.ToString())
                    .Select(n => new { Naam = n, SubSet = delayedContent.FirstOrDefault(c => c.PossibleNummer == n), })
                    .ToArray();
                // Specifieke verzen moeten allemaal gevonden kunnen worden
                if (preSelect.Any(c => c.SubSet == null))
                    return new Oplossing() { Status = DatabaseZoekStatus.VersFout };
                returnValue.Content = preSelect
                    .Select(s => s.SubSet.GetContent())
                    .ToArray();
                // Specifieke verzen moeten allemaal interpreteerbaar zijn
                if (returnValue.Content.Any(c => c == null))
                    return new Oplossing() { Status = DatabaseZoekStatus.VersOnleesbaar };
            }
            returnValue.Status = DatabaseZoekStatus.Opgelost;
            return returnValue;
        }

        private static IEnumerable<int> InterpreteerNummers(IEnumerable<string> nummerSets, IEnumerable<string> avaliableNummers)
        {
            int parseAvaliableNummer = 0;
            var avaliableList = avaliableNummers.Where(s => int.TryParse(s, out parseAvaliableNummer)).Select(s => parseAvaliableNummer).ToArray();
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

        private IEnumerable<IContentDelayed> KrijgContentDelayed(IDbItem vanItem, DbSetSettings metSettings)
        {
            if (_cachedContentDelayed.ContainsKey(vanItem))
                return _cachedContentDelayed[vanItem];

            var content = Array.Empty<IContentDelayed>();
            if (metSettings.ItemsHaveSubContent)
            {
                content = vanItem.Content.TryAccessSubs()
                    .Select(s => new ContentDelayed(KrijgDirecteContent, s.Name.Name, s.Content))
                    .ToArray();
            }
            else if (metSettings.ItemIsSubContent) 
            {
                var contentDirect = KrijgDirecteContent(vanItem.Content, null);
                if (contentDirect.InhoudType == InhoudType.Tekst)
                    content = SplitFile(contentDirect.Inhoud)
                        .Select(s => new ContentDirect(s))
                        .ToArray();
            }
            _cachedContentDelayed.Add(vanItem, content);

            return content;
        }

        private Content KrijgDirecteContent(IDbItemContent metItem, string possibleNummer)
        {
            if (_cachedContentDirect.ContainsKey(metItem))
                return _cachedContentDirect[metItem];

            Content content = null;
            int? nummer = null;
            if (int.TryParse(possibleNummer ?? "", out int parseNummer))
                nummer = parseNummer;
            switch (metItem.Type)
            {
                case "txt":
                    using (var fs = metItem.GetContentStream())
                    {
                        var rdr = new StreamReader(fs, Encoding.Default);
                        // geef de inhoud als tekst terug
                        content = new Content { Inhoud = rdr.ReadToEnd(), InhoudType = InhoudType.Tekst, Nummer = nummer };
                    }
                    break;
                case "ppt":
                case "pptx":
                    // geef de inhoud als link terug
                    content = new Content { Inhoud = metItem.PersistentLink, InhoudType = InhoudType.PptLink, Nummer = nummer };
                    break;
            }
            _cachedContentDirect.Add(metItem, content);

            // Geef een leeg item terug als we het niet konden verwerken
            return content;
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

        private interface IContentDelayed
        {
            string PossibleNummer { get; }
            Content GetContent();
        }
        private class ContentDelayed : IContentDelayed
        {
            public string PossibleNummer { get; private set; }
            private readonly IDbItemContent _dbItem;
            private readonly Func<IDbItemContent, string, Content> _krijgDirecteContent;
            public ContentDelayed(Func<IDbItemContent, string, Content> krijgDirecteContent, string possibleNummer, IDbItemContent dbItem)
            {
                _krijgDirecteContent = krijgDirecteContent;
                PossibleNummer = possibleNummer;
                _dbItem = dbItem;
            }
            public Content GetContent()
            {
                return _krijgDirecteContent(_dbItem, PossibleNummer);
            }
        }
        private class ContentDirect : IContentDelayed
        {
            public string PossibleNummer { get; private set; }
            private readonly Content _content;
            public ContentDirect(Content content)
            {
                PossibleNummer = content.Nummer.ToString();
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

        private class Oplossing : IOplossing
        {
            public DatabaseZoekStatus Status { get; set; }
            public OplossingOnderdeel Onderdeel { get; set; }
            public OplossingOnderdeel Fragment { get; set; }
            public IEnumerable<ILiturgieContent> Content { get; set; }
            public bool ZonderContentSplitsing { get; set; }
            public bool? StandaardNietTonenInLiturgie { get; set; }

            public Oplossing()
            {
                Status = DatabaseZoekStatus.Onbekend;
            }
        }
    }

    public static class MapMasksToLiturgie
    {
        public static IEnumerable<LiturgieMapmaskArg> Map(IEnumerable<IMapmask> masks)
        {
            return masks.Select(m => new LiturgieMapmaskArg { Name = m.Name, RealName = m.RealName }).ToList();
        }
    }


    public static class MapInstellingenToSettings
    {
        public static LiturgieSettings Map(IInstellingen instellingen)
        {
            return new LiturgieSettings
            {
                ToonBijbeltekstenInLiturgie = instellingen.ToonBijbeltekstenInLiturgie,
                GebruikDisplayNameVoorZoeken = instellingen.GebruikDisplayNameVoorZoeken,
            };
        }
    }
}
