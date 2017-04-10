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
        private IEnumerable<IVrijZoekresultaatMogelijkheid> _onderdelenLijstCache;
        private readonly ILiturgieInterpreteer _liturgieInterperator;

        public LiturgieOplosser(ILiturgieDatabase.ILiturgieDatabase database, ILiturgieInterpreteer liturgieInterperator, string defaultSetNameEmpty)
        {
            _database = database;
            _defaultSetNameEmpty = defaultSetNameEmpty;
            _liturgieInterperator = liturgieInterperator;
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
        public IVrijZoekresultaat VrijZoeken(string zoekTekst, IVrijZoekresultaat vorigResultaat = null)
        {
            var veiligeZoekTekst = (zoekTekst ?? "").TrimStart();
            var veranderingGemaakt = false;

            var onderdeelLijst = KrijgBasisDatabaseLijst(true);
            var fragmentLijst = Enumerable.Empty<IVrijZoekresultaatMogelijkheid>();
            var vorigeZoektermSplit = _liturgieInterperator.VanTekstregel(vorigResultaat == null ? "" : vorigResultaat.ZoekTerm);
            var huidigeZoektermSplit = _liturgieInterperator.VanTekstregel(veiligeZoekTekst);

            if ((zoekTekst.Length > 0 && LiturgieInterpretator.InterpreteerLiturgieRuw.BenamingDeelScheidingstekens.Contains(zoekTekst.Last())) || (string.IsNullOrWhiteSpace(vorigeZoektermSplit.Deel) && !string.IsNullOrWhiteSpace(huidigeZoektermSplit.Deel)))
            {
                // Fragment is er bij gekomen
                veranderingGemaakt = true;
                fragmentLijst = ZoekVerdieping(huidigeZoektermSplit.Benaming).Select(t => new ZoekresultaatItem()
                {
                    Weergave = $"{huidigeZoektermSplit.Benaming} {t.Resultaat}",
                    UitDatabase = t.Database,
                }).ToList();
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

        private IEnumerable<IVrijZoekresultaatMogelijkheid> KrijgBasisDatabaseLijst(bool cached)
        {
            if (!cached)
                return ZoekBasisDatabaseLijst()
                    .Select(t => new ZoekresultaatItem()
                    {
                        Weergave = t.Resultaat,
                        UitDatabase = t.Database,
                    });
            else
            {
                if (_onderdelenLijstCache == null)
                    _onderdelenLijstCache = ZoekBasisDatabaseLijst()
                        .Select(t => new ZoekresultaatItem()
                        {
                            Weergave = t.Resultaat,
                            UitDatabase = t.Database,
                        })
                        .ToList();
                return _onderdelenLijstCache;
            }
        }
        private IList<IZoekresultaat> ZoekBasisDatabaseLijst()
        {
            return _database.KrijgAlleOnderdelen()  // Alle onderdelen (psalmen, gezangen, bijbelboeken, etc)
                .Union(ZoekVerdieping(FileEngineDefaults.CommonFilesSetName))  // Alle slide templates zoals amen, votum, bidden etc)
                .Distinct().ToList();
        }
        private IEnumerable<IZoekresultaat> ZoekVerdieping(string vanOnderdeelNaam)
        {
            return _database.KrijgAlleFragmenten(vanOnderdeelNaam);
        }

        private Zoekresultaat ZoekresultaatSamenstellen(string zoekTekst, IVrijZoekresultaat vorigResultaat, IEnumerable<IVrijZoekresultaatMogelijkheid> lijst, bool lijstIsGewijzigd)
        {
            var zoekLijst = Enumerable.Empty<IVrijZoekresultaatMogelijkheid>();
            var zoekLijstDeltaToegevoegd = Enumerable.Empty<IVrijZoekresultaatMogelijkheid>();
            var zoekLijstDeltaVerwijderd = Enumerable.Empty<IVrijZoekresultaatMogelijkheid>();
            var aanpassing = VrijZoekresultaatAanpassingType.Alles;

            if (!lijstIsGewijzigd && vorigResultaat != null)
            {
                aanpassing = VrijZoekresultaatAanpassingType.Geen;
                zoekLijst = vorigResultaat.AlleMogelijkheden.ToList();
            }
            else
            {
                zoekLijst = lijst.Distinct().OrderBy(z => z.Weergave).ToList();
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

        // TODO samenstellen tekst om op te zoeken staat op meerder plekken
        public string MaakLiturgieregelVanZoekresultaat(string invoerTekst, IVrijZoekresultaat zoekresultaat)
        {
            if (zoekresultaat == null || invoerTekst == null)
                return invoerTekst;
            var invoerTekstSplitsing = _liturgieInterperator.VanTekstregel(invoerTekst);
            var teZoekenTekst = $"{invoerTekstSplitsing.Benaming} {invoerTekstSplitsing.Deel}";
            var itemInZoeklijst = zoekresultaat.AlleMogelijkheden.FirstOrDefault(z => z.Weergave == teZoekenTekst);
            if (itemInZoeklijst == null)
                return invoerTekst;
            if (itemInZoeklijst.UitDatabase == Database.LiturgieDatabaseSettings.DatabaseNameBijbeltekst)
                return $"{invoerTekst} (als:bijbeltekst)";
            return invoerTekst;
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
            public IEnumerable<IVrijZoekresultaatMogelijkheid> AlleMogelijkheden { get; set; }
            public IEnumerable<IVrijZoekresultaatMogelijkheid> DeltaMogelijkhedenToegevoegd { get; set; }
            public IEnumerable<IVrijZoekresultaatMogelijkheid> DeltaMogelijkhedenVerwijderd { get; set; }

            public VrijZoekresultaatAanpassingType ZoeklijstAanpassing { get; set; }

            public string ZoekTerm { get; set; }
        }

        private class ZoekresultaatItem : IVrijZoekresultaatMogelijkheid
        {
            public string Weergave { get; set; }
            public string UitDatabase { get; set; }

            public bool Equals(IVrijZoekresultaatMogelijkheid x, IVrijZoekresultaatMogelijkheid y)
            {
                if (x == null || y == null)
                    return false;
                return x.Weergave == y.Weergave;
            }

            public int GetHashCode(IVrijZoekresultaatMogelijkheid obj)
            {
                return obj.Weergave.GetHashCode();
            }

            public override string ToString()
            {
                return Weergave;
            }
        }
    }
}
