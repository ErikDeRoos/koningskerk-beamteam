// Copyright 2024 door Erik de Roos
using Generator.Database;
using Generator.Database.FileSystem;
using Generator.Database.Models;
using Generator.LiturgieInterpretator.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Generator.LiturgieInterpretator
{
    /// <summary>
    /// Zoek naar de opgegeven liturgieen.
    /// </summary>
    public class LiturgieZoeker : ILiturgieZoeken
    {
        private readonly ILiturgieDatabaseZoek _databaseZoek;
        private readonly ILiturgieTekstNaarObject _liturgieTekstNaarObject;

        private IEnumerable<IVrijZoekresultaatMogelijkheid> _onderdelenLijstCache;
        private ZoekRestricties _onderdelenLijstRestrictiesCache;

        public LiturgieZoeker(ILiturgieDatabaseZoek databaseZoek, ILiturgieTekstNaarObject liturgieTekstNaarObject)
        {
            _databaseZoek = databaseZoek;
            _liturgieTekstNaarObject = liturgieTekstNaarObject;
        }

        /// <summary>
        /// Doe een 'voor check' op de zoekopdracht en geef aan of er een ander resultaat verwacht kan worden
        /// </summary>
        public bool GaatVrijZoekenAnderResultaatGeven(string zoekTekst, bool alsBijbeltekst = false, IVrijZoekresultaat vorigResultaat = null)
        {
            var veiligeZoekTekst = (zoekTekst ?? "").TrimStart();

            // Los van het detecteren van de verandering voor de 'hele lijst terug geven', kunnen we ook bepalen of er daadwerkelijk een verandering is
            if (vorigResultaat != null && vorigResultaat.AlsBijbeltekst == alsBijbeltekst && veiligeZoekTekst == vorigResultaat.ZoekTerm)
                return false;

            return true;
        }

        /// <summary>
        /// Dit maakt een lijst van resultaten die voldoen aan de zoektekst. Filteren gebeurd hier niet maar in de UI zelf.
        /// We helpen de UX beleving door de UI niet direct alle mogelijkheden meteen terug te geven, alleen te verdiepen waar de gebruiker
        /// echt naar zoekt.
        /// Verder wordt aangegeven welke zoekresultaten veranderd zijn zodat de UI de verwijderde of toegevoegde elementen kan animeren 
        /// of iets dergelijks.
        /// </summary>
        public IVrijZoekresultaat VrijZoeken(string zoekTekst, bool alsBijbeltekst = false, IVrijZoekresultaat vorigResultaat = null)
        {
            // Let op: Omdat de UI zelf filtert detecteren we hier alleen overgangen.
            // Oftewel, wij geven een complete lijst terug en de UI filtert zelf.

            var veiligeZoekTekst = (zoekTekst ?? "").TrimStart();
            var veranderingGemaakt = vorigResultaat == null;
            var zoekRestricties = new ZoekRestricties(alsBijbeltekst);
            var aanname = vorigResultaat?.Aanname;
            var laatsteZoektekenIsFragmentWissel = veiligeZoekTekst.Length > 0 ? LiturgieInterpretator.LiturgieTekstNaarObject.BenamingDeelScheidingstekens.Contains(veiligeZoekTekst.Last()) : false;

            var onderdeelLijst = KrijgBasisDatabaseLijst(zoekRestricties, true);
            var fragmentLijst = Enumerable.Empty<IVrijZoekresultaatMogelijkheid>();
            var vorigeZoektermSplit = _liturgieTekstNaarObject.VanTekstregel(vorigResultaat == null ? "" : vorigResultaat.ZoekTerm);
            var huidigeZoektermSplit = _liturgieTekstNaarObject.VanTekstregel(veiligeZoekTekst);

            // Er is een wijziging er een overgang is van bijbeltekst naar normale tekst of andersom
            if (vorigResultaat != null && vorigResultaat.AlsBijbeltekst != alsBijbeltekst)
            {
                veranderingGemaakt = true;
            }

            // Kijk of er in de zoektekst een spatie is gebruikt, dan komt er nu een overgang aan
            if ((veiligeZoekTekst.Length > 0 && laatsteZoektekenIsFragmentWissel) || (string.IsNullOrWhiteSpace(vorigeZoektermSplit.Deel) && !string.IsNullOrWhiteSpace(huidigeZoektermSplit.Deel)))
            {
                // Fragment is er bij gekomen
                veranderingGemaakt = true;
                fragmentLijst = ZoekFragmenten(zoekRestricties, huidigeZoektermSplit.Benaming).Select(t => new ZoekresultaatItem()
                {
                    Weergave = $"{huidigeZoektermSplit.Benaming} {t.Resultaat.Weergave}",
                    UitDatabase = t.Database.Weergave,
                }).ToList();

                // Geen aannames meer
                aanname = null;
            }
            // Zo gauw je de spatie weghaald is de overgang weer weg
            else if ((veiligeZoekTekst.Length == 0 || !laatsteZoektekenIsFragmentWissel) && string.IsNullOrWhiteSpace(huidigeZoektermSplit.Deel) && (vorigResultaat != null && vorigResultaat.ZoekTerm.Length > 0 && LiturgieInterpretator.LiturgieTekstNaarObject.BenamingDeelScheidingstekens.Contains(vorigResultaat.ZoekTerm.Last())))
            {
                // Fragment is weer weg gehaald
                veranderingGemaakt = true;
            }

            // Bepaal het nieuwe zoekresultaat
            var nieuwResultaat = ZoekresultaatSamenstellen(veiligeZoekTekst, alsBijbeltekst, vorigResultaat, onderdeelLijst.Union(fragmentLijst), aanname, veranderingGemaakt);
            if (vorigResultaat == null)
                return nieuwResultaat;

            // Het kan zijn dat bij het vorige zoekresultaat nog meerdere opties mogelijk waren, maar dat er nu nog maar 1 optie mogelijk is,
            // terwijl je nog wel een paar tekens moet typen. Dat kan handiger,  we maken de aanname dat je deze ene optie bedoelt:
            if (!laatsteZoektekenIsFragmentWissel && vorigResultaat.Aanname == null && VoorspelZoekresultaat(vorigResultaat.AlleMogelijkheden, vorigResultaat.ZoekTerm).Count() > 1 && VoorspelZoekresultaat(nieuwResultaat.AlleMogelijkheden, nieuwResultaat.ZoekTerm).Count() == 1)
            {
                veranderingGemaakt = true;
                aanname = VoorspelZoekresultaat(nieuwResultaat.AlleMogelijkheden, nieuwResultaat.ZoekTerm).First().Weergave;

                // Fragment toevoegen op basis van aanname
                fragmentLijst = ZoekFragmenten(zoekRestricties, aanname).Select(t => new ZoekresultaatItem()
                {
                    Weergave = $"{aanname} {t.Resultaat.Weergave}",
                    UitDatabase = t.Database.Weergave,
                }).ToList();

                nieuwResultaat = ZoekresultaatSamenstellen(veiligeZoekTekst, alsBijbeltekst, vorigResultaat, onderdeelLijst.Union(fragmentLijst), aanname, veranderingGemaakt);
            }
            // Als de aanname niet meer gemaakt kan worden
            else if (!string.IsNullOrEmpty(vorigResultaat.Aanname) && VoorspelZoekresultaat(nieuwResultaat.AlleMogelijkheden, nieuwResultaat.ZoekTerm).Count() > VoorspelZoekresultaat(vorigResultaat.AlleMogelijkheden, vorigResultaat.ZoekTerm).Count())
            {
                veranderingGemaakt = true;
                aanname = null;

                nieuwResultaat = ZoekresultaatSamenstellen(veiligeZoekTekst, alsBijbeltekst, vorigResultaat, onderdeelLijst.Union(fragmentLijst), aanname, veranderingGemaakt);
            }

            return nieuwResultaat;
        }

        private IEnumerable<IVrijZoekresultaatMogelijkheid> VoorspelZoekresultaat(IEnumerable<IVrijZoekresultaatMogelijkheid> resultaten, string zoekTekst)
        {
            return resultaten.Where(r => r.Weergave.StartsWith(zoekTekst, StringComparison.CurrentCultureIgnoreCase));
        }


        private IEnumerable<IVrijZoekresultaatMogelijkheid> KrijgBasisDatabaseLijst(ZoekRestricties zoekRestricties, bool cached)
        {
            if (!cached)
                return ZoekBasisDatabaseLijst(zoekRestricties)
                    .Select(t => new ZoekresultaatItem()
                    {
                        Weergave = t.Resultaat.Weergave,
                        VeiligeNaam = t.Resultaat.VeiligeNaam,
                        UitDatabase = t.Database.Weergave,
                    })
                    .Distinct().ToList();
            else
            {
                if (_onderdelenLijstCache == null || !zoekRestricties.Equals(_onderdelenLijstRestrictiesCache))
                {
                    _onderdelenLijstCache = KrijgBasisDatabaseLijst(zoekRestricties, false);
                    _onderdelenLijstRestrictiesCache = zoekRestricties;
                }
                return _onderdelenLijstCache;
            }
        }

        /// <summary>
        /// Krijg de basislijst. Dus alle set namen. 
        /// En indien gewenst ook de inhoud van de 'common' set.
        /// </summary>
        private IList<IZoekresultaat> ZoekBasisDatabaseLijst(ZoekRestricties zoekRestricties)
        {
            var alleDatabases = Enumerable.Empty<IZoekresultaat>();

            // zoekrestricties toepassen
            if (zoekRestricties.ZoekInBijbel && !zoekRestricties.ZoekInLiederen)
                alleDatabases = _databaseZoek.KrijgAlleSetNamenInBijbelDb();
            else if (!zoekRestricties.ZoekInBijbel && zoekRestricties.ZoekInLiederen)
                alleDatabases = _databaseZoek.KrijgAlleSetNamenInNormaleDb();
            else if (zoekRestricties.ZoekInBijbel && zoekRestricties.ZoekInLiederen)
                alleDatabases = _databaseZoek.KrijgAlleSetNamen();

            // Alle slide templates zoals amen, votum, bidden etc)
            if (zoekRestricties.ZoekInCommon)
                alleDatabases = alleDatabases.Concat(_databaseZoek.KrijgAlleFragmentenUitNormaleDb(FileEngineDefaults.CommonFilesSetName));  

            return alleDatabases.ToList();
        }

        private IList<IZoekresultaat> ZoekFragmenten(ZoekRestricties zoekRestricties, string setNaam)
        {
            var alleFragmenten = Enumerable.Empty<IZoekresultaat>();

            // zoekrestricties toepassen
            if (zoekRestricties.ZoekInBijbel && !zoekRestricties.ZoekInLiederen)
                alleFragmenten = _databaseZoek.KrijgAlleFragmentenUitBijbelDb(setNaam);
            else if (!zoekRestricties.ZoekInBijbel && zoekRestricties.ZoekInLiederen)
                alleFragmenten = _databaseZoek.KrijgAlleFragmentenUitNormaleDb(setNaam);
            else if (zoekRestricties.ZoekInBijbel && zoekRestricties.ZoekInLiederen)
                alleFragmenten = _databaseZoek.KrijgAlleFragmentenUitAlleDatabases(setNaam);

            return alleFragmenten.ToList();
        }


        private Zoekresultaat ZoekresultaatSamenstellen(string zoekTekst, bool alsBijbeltekst, IVrijZoekresultaat vorigResultaat, IEnumerable<IVrijZoekresultaatMogelijkheid> lijst, string aanname, bool lijstIsGewijzigd)
        {
            var zoekLijst = Enumerable.Empty<IVrijZoekresultaatMogelijkheid>();
            var zoekLijstDeltaToegevoegd = Enumerable.Empty<IVrijZoekresultaatMogelijkheid>();
            var zoekLijstDeltaVerwijderd = Enumerable.Empty<IVrijZoekresultaatMogelijkheid>();
            var aanpassing = VrijZoekresultaatAanpassingType.Alles;
            var vermoedelijkeDatabase = string.Empty;

            if (!lijstIsGewijzigd && vorigResultaat != null)
            {
                aanpassing = VrijZoekresultaatAanpassingType.Geen;
                zoekLijst = vorigResultaat.AlleMogelijkheden.ToList();
                vermoedelijkeDatabase = vorigResultaat.VermoedelijkeDatabase;
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

                var databases = zoekLijst.Select(z => z.UitDatabase).Distinct().ToList();
                if (databases.Count == 1)
                    vermoedelijkeDatabase = databases.First();
            }

            return new Zoekresultaat()
            {
                ZoekTerm = zoekTekst,
                AlsBijbeltekst = alsBijbeltekst,
                Aanname = aanname,
                VermoedelijkeDatabase = vermoedelijkeDatabase,
                AlleMogelijkheden = zoekLijst.ToList(),
                DeltaMogelijkhedenToegevoegd = zoekLijstDeltaToegevoegd,
                DeltaMogelijkhedenVerwijderd = zoekLijstDeltaVerwijderd,
                ZoeklijstAanpassing = aanpassing,
            };
        }

        public LiturgieOptiesGebruiker ZoekStandaardOptiesUitZoekresultaat(string invoerTekst, IVrijZoekresultaat zoekresultaat)
        {
            if (string.IsNullOrWhiteSpace(invoerTekst))
                return null;
            var databaseNaam = string.Empty;
            if (zoekresultaat != null)
            {
                var invoerTekstSplitsing = _liturgieTekstNaarObject.VanTekstregel(invoerTekst);
                var teZoekenTekst = $"{invoerTekstSplitsing.Benaming} {invoerTekstSplitsing.Deel}";
                var itemInZoeklijst = zoekresultaat.AlleMogelijkheden.FirstOrDefault(z => z.Weergave == teZoekenTekst);
                if (itemInZoeklijst != null)
                    databaseNaam = itemInZoeklijst.UitDatabase;
                if (string.IsNullOrWhiteSpace(databaseNaam))
                    databaseNaam = zoekresultaat.VermoedelijkeDatabase;
            }
            return _liturgieTekstNaarObject.BepaalBasisOptiesTekstinvoer(invoerTekst, databaseNaam);
        }

        public LiturgieOptiesGebruiker ToonOpties(string optiesInTekst)
        {
            return _liturgieTekstNaarObject.BepaalOptiesTekstinvoer(optiesInTekst);
        }


        public string MaakTotTekst(string invoerTekst, LiturgieOptiesGebruiker opties, IVrijZoekresultaat zoekresultaat)
        {
            var tekstUitOpties = _liturgieTekstNaarObject.MaakTekstVanOpties(opties);
            var gebruiktZoekresultaat = zoekresultaat.AlleMogelijkheden
                .Where(w => invoerTekst.StartsWith(w.Weergave))
                .OrderByDescending(w => w.Weergave.Length)
                .FirstOrDefault();
            if (gebruiktZoekresultaat == null)
            {
                return $"{invoerTekst.Trim()} {tekstUitOpties.Trim()}".Trim();
            }
            else
            {
                var resterend = invoerTekst.Substring(gebruiktZoekresultaat.Weergave.Length);
                return $"{gebruiktZoekresultaat.VeiligeNaam} {resterend.Trim()} {tekstUitOpties.Trim()}".Trim();
            }
        }

        public string[] SplitsVoorOpties(string liturgieRegel)
        {
            return _liturgieTekstNaarObject.SplitsVoorOpties(liturgieRegel);
        }

        private class Zoekresultaat : IVrijZoekresultaat
        {
            public string ZoekTerm { get; set; }
            public bool AlsBijbeltekst { get; set; }
            public string VermoedelijkeDatabase { get; set; }
            public string Aanname { get; set; }

            public IEnumerable<IVrijZoekresultaatMogelijkheid> AlleMogelijkheden { get; set; }
            public IEnumerable<IVrijZoekresultaatMogelijkheid> DeltaMogelijkhedenToegevoegd { get; set; }
            public IEnumerable<IVrijZoekresultaatMogelijkheid> DeltaMogelijkhedenVerwijderd { get; set; }

            public VrijZoekresultaatAanpassingType ZoeklijstAanpassing { get; set; }
        }

        private class ZoekresultaatItem : IVrijZoekresultaatMogelijkheid
        {
            public string Weergave { get; set; }
            public string VeiligeNaam { get; set; }
            public string UitDatabase { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is IVrijZoekresultaatMogelijkheid vrijZoekresultaat)
                    return Equals(this, vrijZoekresultaat);

                return base.Equals(obj);
            }

            public bool Equals(IVrijZoekresultaatMogelijkheid x, IVrijZoekresultaatMogelijkheid y)
            {
                if (x == null || y == null)
                    return false;
                return x.Weergave.Equals(y.Weergave, System.StringComparison.InvariantCultureIgnoreCase);  // Alleen equals checks op weergave naam
            }

            public override string ToString()
            {
                return Weergave;
            }
        }

        private class ZoekRestricties : IEquatable<ZoekRestricties>
        {
            public bool ZoekInBijbel { get; }
            public bool ZoekInCommon { get; }
            public bool ZoekInLiederen { get; }

            public ZoekRestricties(bool alsBijbeltekst)
            {
                ZoekInBijbel = alsBijbeltekst;
                ZoekInCommon = !alsBijbeltekst;
                ZoekInLiederen = !alsBijbeltekst;
            }

            public bool Equals(ZoekRestricties other)
            {
                return other != null &&
                    ZoekInBijbel == other.ZoekInBijbel &&
                    ZoekInCommon == other.ZoekInCommon &&
                    ZoekInLiederen == other.ZoekInLiederen;
            }
        }
    }
}
