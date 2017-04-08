// Copyright 2017 door Erik de Roos
using Generator.Database.FileSystem;
using ILiturgieDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.String;

namespace Generator.LiturgieOplosser
{
    public static class LiturgieOplosserSettings
    {
        public const string OptieNietVerwerken = "geendb";
        public const string OptieNietTonenInVolgende = "geenvolg";
        public const string OptieNietTonenInOverzicht = "geenlt";
        public const string OptieAlternatieveNaamOverzicht = "altlt";
        public const string OptieAlternatieveNaam = "altnm";
    }

    /// <summary>
    /// Zoek naar de opgegeven liturgieen.
    /// </summary>
    public class LiturgieOplosser : ILiturgieLosOp
    {
        private readonly ILiturgieDatabase.ILiturgieDatabase _database;
        private readonly string _defaultSetNameEmpty;
        private IEnumerable<string> _onderdelenLijstCache;

        public LiturgieOplosser(ILiturgieDatabase.ILiturgieDatabase database, string defaultSetNameEmpty)
        {
            _database = database;
            _defaultSetNameEmpty = defaultSetNameEmpty;
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
            regel.VerwerkenAlsSlide = !trimmedOpties.Any(o => o.StartsWith(LiturgieOplosserSettings.OptieNietVerwerken, StringComparison.CurrentCultureIgnoreCase));
            regel.TonenInVolgende = !trimmedOpties.Any(o => o.StartsWith(LiturgieOplosserSettings.OptieNietTonenInVolgende, StringComparison.CurrentCultureIgnoreCase));
            regel.TonenInOverzicht = !trimmedOpties.Any(o => o.StartsWith(LiturgieOplosserSettings.OptieNietTonenInOverzicht, StringComparison.CurrentCultureIgnoreCase));

            // regel visualisatie default
            regel.DisplayEdit.Naam = item.Benaming;
            regel.DisplayEdit.SubNaam = item.Deel;
            regel.DisplayEdit.VersenGebruikDefault = new VersenDefault();

            // zoek de regels in de database en pak ook de naamgeving daar uit over
            if (regel.VerwerkenAlsSlide)
            {
                var fout = Aanvullen(regel, item);
                if (fout.HasValue)
                    return new Oplossing(fout.Value, item);
            } else
            {
                regel.VerwerkenAlsType = VerwerkingType.nietverwerken;
                regel.DisplayEdit.VersenGebruikDefault = new VersenDefault(item.VerzenZoalsIngevoerd);
            }

            // Check of er een mask is (mooiere naam)
            // Anders underscores als spaties tonen
            var maskCheck = masks?.FirstOrDefault(m => Compare(m.RealName, regel.DisplayEdit.Naam, true) == 0);
            if (maskCheck != null)
                regel.DisplayEdit.Naam = maskCheck.Name;
            else
                regel.DisplayEdit.Naam = (regel.DisplayEdit.Naam ?? "").Replace("_", " ");
            regel.DisplayEdit.SubNaam = (regel.DisplayEdit.SubNaam ?? "").Replace("_", " ");
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
            var optieMetAltNaamOverzicht = GetOptieParam(trimmedOpties, LiturgieOplosserSettings.OptieAlternatieveNaamOverzicht);
            if (!IsNullOrWhiteSpace(optieMetAltNaamOverzicht))
            {
                regel.DisplayEdit.NaamOverzicht = optieMetAltNaamOverzicht;
                regel.DisplayEdit.SubNaam = null;
            }
            var optieMetAltNaamVolgende = GetOptieParam(trimmedOpties, LiturgieOplosserSettings.OptieAlternatieveNaam);
            if (!IsNullOrWhiteSpace(optieMetAltNaamVolgende))
            {
                regel.DisplayEdit.Naam = optieMetAltNaamVolgende;
                regel.DisplayEdit.SubNaam = null;
            }

            // geef de oplossing terug
            return new Oplossing(LiturgieOplossingResultaat.Opgelost, item, regel);
        }

        private LiturgieOplossingResultaat? Aanvullen(Regel regel, ILiturgieInterpretatie item)
        {
            var setNaam = item.Benaming;
            if (item is ILiturgieInterpretatieBijbeltekst)
            {
                regel.DisplayEdit.VersenGebruikDefault = new VersenDefault(string.Empty);
                return BijbeltekstAanvuller(regel, setNaam, (item as ILiturgieInterpretatieBijbeltekst).PerDeelVersen.ToList());
            }
            var zoekNaam = item.Deel;
            if (IsNullOrEmpty(item.Deel))
            {
                setNaam = FileEngineDefaults.CommonFilesSetName;
                zoekNaam = item.Benaming;
                regel.TonenInOverzicht = false;  // TODO tijdelijk default gedrag van het niet tonen van algemene items in het overzicht overgenomen uit de oude situatie
            }

            return NormaleAanvuller(regel, setNaam, zoekNaam, item.Verzen.ToList());
        }
        private LiturgieOplossingResultaat? NormaleAanvuller(Regel regel, string setNaam, string zoekNaam, IEnumerable<string> verzen)
        {
            regel.VerwerkenAlsType = VerwerkingType.normaal;
            var verzenList = verzen.ToList();
            var resultaat = _database.ZoekOnderdeel(setNaam, zoekNaam, verzenList);
            if (resultaat.Status != LiturgieOplossingResultaat.Opgelost)
                return resultaat.Status;

            if (resultaat.OnderdeelNaam == FileEngineDefaults.CommonFilesSetName)
            {
                regel.DisplayEdit.Naam = resultaat.FragmentNaam;
                regel.DisplayEdit.VersenGebruikDefault = new VersenDefault(string.Empty);  // Expliciet: Common bestanden hebben nooit versen
            }
            else {
                regel.DisplayEdit.Naam = resultaat.OnderdeelNaam;
                regel.DisplayEdit.SubNaam = resultaat.FragmentNaam;
            }
            regel.Content = resultaat.Content.ToList();
            if (resultaat.ZonderContentSplitsing)
                regel.DisplayEdit.VersenGebruikDefault = new VersenDefault(string.Empty);  // Altijd default gebruiken omdat er altijd maar 1 content is
            regel.DisplayEdit.VolledigeContent = !verzenList.Any();

            // bepaal de naamgeving
            if (!IsNullOrWhiteSpace(resultaat.OnderdeelDisplayNaam))
                regel.DisplayEdit.Naam = resultaat.OnderdeelDisplayNaam.Equals(_defaultSetNameEmpty, StringComparison.CurrentCultureIgnoreCase) ? null : resultaat.OnderdeelDisplayNaam;

            return null;
        }
        private LiturgieOplossingResultaat? BijbeltekstAanvuller(Regel regel, string setNaam, IEnumerable<ILiturgieInterpretatieBijbeltekstDeel> versDelen)
        {
            regel.VerwerkenAlsType = VerwerkingType.bijbeltekst;
            var content = new List<ILiturgieContent>();
            var versDelenLijst = versDelen.ToList();
            foreach(var deel in versDelenLijst)
            {
                var resultaat = _database.ZoekOnderdeel(VerwerkingType.bijbeltekst, setNaam, deel.Deel, deel.Verzen);
                if (resultaat.Status != LiturgieOplossingResultaat.Opgelost)
                    return resultaat.Status;
                content.AddRange(resultaat.Content);
                // let op, naamgeving wordt buitenom geregeld
            }
            regel.Content = content.ToList();
            regel.DisplayEdit.VolledigeContent = versDelenLijst.Count == 1 && !versDelen.FirstOrDefault().Verzen.Any();
            return null;
        }

        private static string GetOptieParam(IEnumerable<string> opties, string optie)
        {
            var optieMetParam = opties.FirstOrDefault(o => o.StartsWith(optie, StringComparison.CurrentCultureIgnoreCase));
            if (optieMetParam == null || optieMetParam.Length == optie.Length)
                return string.Empty;
            return optieMetParam.Substring(optie.Length + 1).Trim();
        }

        public IEnumerable<ILiturgieOplossing> LosOp(IEnumerable<ILiturgieInterpretatie> items, IEnumerable<ILiturgieMapmaskArg> masks)
        {
            return items.Select(i => LosOp(i, masks)).ToList();
        }

        // TODO oplossen dat logica om regel weer samen te stellen uit gaat van vaste waarden
        // TODO efficienter omgaan met zoekresources (verschillende lijsten opslaan in zoekresultaat en alleen wijzigingen veranderen)
        public IVrijZoekresultaat VrijZoeken(string zoekTekst, ILiturgieInterpreteer liturgieInterperator, IVrijZoekresultaat vorigResultaat = null)
        {
            var veiligeZoekTekst = (zoekTekst ?? "").TrimStart();
            var veranderingGemaakt = false;

            var onderdeelLijst = KrijgBasisDatabaseLijst(true);
            var fragmentLijst = Enumerable.Empty<string>();
            var vorigeZoektermSplit = liturgieInterperator.VanTekstregel(vorigResultaat == null ? "" : vorigResultaat.ZoekTerm);
            var huidigeZoektermSplit = liturgieInterperator.VanTekstregel(veiligeZoekTekst);

            if ((zoekTekst.Length > 0 && LiturgieInterpretator.InterpreteerLiturgieRuw.BenamingDeelScheidingstekens.Contains(zoekTekst.Last())) || (string.IsNullOrWhiteSpace(vorigeZoektermSplit.Deel) && !string.IsNullOrWhiteSpace(huidigeZoektermSplit.Deel)))
            {
                // Fragment is er bij gekomen
                veranderingGemaakt = true;
                fragmentLijst = ZoekVerdieping(huidigeZoektermSplit.Benaming).Select(t => $"{huidigeZoektermSplit.Benaming} {t}").ToList();
            }
            else if (!string.IsNullOrWhiteSpace(vorigeZoektermSplit.Deel) && string.IsNullOrWhiteSpace(huidigeZoektermSplit.Deel) && (zoekTekst.Length == 0 || !LiturgieInterpretator.InterpreteerLiturgieRuw.BenamingDeelScheidingstekens.Contains(zoekTekst.Last())))
            {
                // Fragment is weer weg gehaald
                veranderingGemaakt = true;
            }
            else if (vorigResultaat == null)
            {
                veranderingGemaakt = true;
            }

            return ZoekresultaatSamenstellen(veiligeZoekTekst, vorigResultaat, onderdeelLijst.Union(fragmentLijst), veranderingGemaakt);
        }

        private IEnumerable<string> KrijgBasisDatabaseLijst(bool cached)
        {
            if (!cached)
                return ZoekBasisDatabaseLijst();
            else
            {
                if (_onderdelenLijstCache == null)
                    _onderdelenLijstCache = ZoekBasisDatabaseLijst();
                return _onderdelenLijstCache;
            }
        }
        private IList<string> ZoekBasisDatabaseLijst()
        {
            return _database.KrijgAlleOnderdelen()  // Alle onderdelen (psalmen, gezangen, bijbelboeken, etc)
                .Union(ZoekVerdieping(FileEngineDefaults.CommonFilesSetName))  // Alle slide templates zoals amen, votum, bidden etc)
                .Distinct().ToList();
        }
        private IEnumerable<string> ZoekVerdieping(string vanOnderdeelNaam)
        {
            return _database.KrijgAlleFragmenten(vanOnderdeelNaam);
        }

        private Zoekresultaat ZoekresultaatSamenstellen(string zoekTekst, IVrijZoekresultaat vorigResultaat, IEnumerable<string> lijst, bool lijstIsGewijzigd)
        {
            var zoekLijst = Enumerable.Empty<string>();
            var zoekLijstDeltaToegevoegd = Enumerable.Empty<string>();
            var zoekLijstDeltaVerwijderd = Enumerable.Empty<string>();
            var aanpassing = VrijZoekresultaatAanpassingType.Alles;

            if (!lijstIsGewijzigd && vorigResultaat != null)
            {
                aanpassing = VrijZoekresultaatAanpassingType.Geen;
                zoekLijst = vorigResultaat.AlleMogelijkheden;
            }
            else
            {
                zoekLijst = lijst.Distinct().OrderBy(name => name).ToList();
                if (aanpassing == VrijZoekresultaatAanpassingType.Alles && vorigResultaat != null)
                {
                    zoekLijstDeltaToegevoegd = zoekLijst.Where(z => !vorigResultaat.AlleMogelijkheden.Contains(z)).ToList();
                    zoekLijstDeltaVerwijderd = vorigResultaat.AlleMogelijkheden.Where(z => !zoekLijst.Contains(z)).ToList();
                    if (zoekLijstDeltaVerwijderd.Count() != vorigResultaat.AlleMogelijkheden.Count())
                        aanpassing = VrijZoekresultaatAanpassingType.Deel;
                }
            }

            return new Zoekresultaat()
            {
                ZoekTerm = zoekTekst,
                AlleMogelijkheden = zoekLijst.ToList(),
                DeltaMogelijkhedenToegevoegd = zoekLijstDeltaToegevoegd,
                DeltaMogelijkhedenVerwijderd = zoekLijstDeltaVerwijderd,
                ZoeklijstAanpassing = aanpassing,
            };
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
            public VerwerkingType VerwerkenAlsType { get; set; }

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

        private class Zoekresultaat : IVrijZoekresultaat
        {
            public IEnumerable<string> AlleMogelijkheden { get; set; }
            public IEnumerable<string> DeltaMogelijkhedenToegevoegd { get; set; }
            public IEnumerable<string> DeltaMogelijkhedenVerwijderd { get; set; }

            public VrijZoekresultaatAanpassingType ZoeklijstAanpassing { get; set; }

            public string ZoekTerm { get; set; }
        }
    }
}
