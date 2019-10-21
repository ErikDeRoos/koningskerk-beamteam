﻿// Copyright 2019 door Erik de Roos
using Generator.Database.FileSystem;
using ILiturgieDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.String;

namespace Generator.LiturgieOplosser
{
    /// <summary>
    /// Zoek naar de opgegeven liturgieen.
    /// </summary>
    public class LiturgieOplosser : ILiturgieLosOp
    {
        private readonly ILiturgieDatabase.ILiturgieDatabase _database;
        private readonly string _defaultSetNameEmpty;
        private readonly ILiturgieInterpreteer _liturgieInterperator;

        private IEnumerable<IVrijZoekresultaatMogelijkheid> _onderdelenLijstCache;
        private ZoekRestricties _onderdelenLijstRestrictiesCache;

        public LiturgieOplosser(ILiturgieDatabase.ILiturgieDatabase database, ILiturgieInterpreteer liturgieInterperator, string defaultSetNameEmpty)
        {
            _database = database;
            _defaultSetNameEmpty = defaultSetNameEmpty;
            _liturgieInterperator = liturgieInterperator;
        }


        public ILiturgieOplossing LosOp(ILiturgieInterpretatie item, ILiturgieSettings settings)
        {
            return LosOp(item, null, settings);
        }
        public ILiturgieOplossing LosOp(ILiturgieInterpretatie item, IEnumerable<ILiturgieMapmaskArg> masks, ILiturgieSettings settings)
        {
            var regel = new Regel {DisplayEdit = new RegelDisplay()};

            // verwerk de opties
            regel.VerwerkenAlsSlide = !item.OptiesGebruiker.NietVerwerkenViaDatabase;
            regel.TonenInOverzicht = item.OptiesGebruiker.ToonInOverzicht ?? (item.OptiesGebruiker.AlsBijbeltekst ? settings.ToonBijbeltekstenInLiturgie : true);
            regel.TonenInVolgende = item.OptiesGebruiker.ToonInVolgende ?? true;

            // regel visualisatie default
            regel.DisplayEdit.Naam = item.Benaming;
            regel.DisplayEdit.SubNaam = item.Deel;
            regel.DisplayEdit.VersenGebruikDefault = new VersenDefault();

            // zoek de regels in de database en pak ook de naamgeving daar uit over
            if (regel.VerwerkenAlsSlide)
            {
                var fout = Aanvullen(regel, item, settings);
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
            // kijk of de systeem opties nog iets zeggen over alternatieve naamgeving
            if (!IsNullOrWhiteSpace(item.TeTonenNaamOpOverzicht))
            {
                regel.DisplayEdit.NaamOverzicht = item.TeTonenNaamOpOverzicht;
                regel.DisplayEdit.SubNaam = null;
            }
            if (!IsNullOrWhiteSpace(item.OptiesGebruiker.AlternatieveNaamOverzicht))
            {
                regel.DisplayEdit.NaamOverzicht = item.OptiesGebruiker.AlternatieveNaamOverzicht;
                regel.DisplayEdit.SubNaam = null;
            }
            // kijk of de gebruiker opties nog iets zeggen over alternatieve naamgeving
            if (!IsNullOrWhiteSpace(item.TeTonenNaam))
            {
                regel.DisplayEdit.Naam = item.TeTonenNaam;
                regel.DisplayEdit.SubNaam = null;
            }
            if (!IsNullOrWhiteSpace(item.OptiesGebruiker.AlternatieveNaam))
            {
                regel.DisplayEdit.Naam = item.OptiesGebruiker.AlternatieveNaam;
                regel.DisplayEdit.SubNaam = null;
            }

            // geef de oplossing terug
            return new Oplossing(LiturgieOplossingResultaat.Opgelost, item, regel);
        }

        private LiturgieOplossingResultaat? Aanvullen(Regel regel, ILiturgieInterpretatie item, ILiturgieSettings settings)
        {
            var setNaam = item.Benaming;
            if (item is ILiturgieInterpretatieBijbeltekst)
            {
                regel.DisplayEdit.VersenGebruikDefault = new VersenDefault(string.Empty);
                return BijbeltekstAanvuller(regel, setNaam, (item as ILiturgieInterpretatieBijbeltekst).PerDeelVersen.ToList(), settings);
            }
            var zoekNaam = item.Deel;
            if (IsNullOrEmpty(item.Deel))
            {
                setNaam = FileEngineDefaults.CommonFilesSetName;
                zoekNaam = item.Benaming;
            }

            return NormaleAanvuller(regel, setNaam, zoekNaam, item.Verzen.ToList(), settings);
        }
        private LiturgieOplossingResultaat? NormaleAanvuller(Regel regel, string setNaam, string zoekNaam, IEnumerable<string> verzen, ILiturgieSettings settings)
        {
            regel.VerwerkenAlsType = VerwerkingType.normaal;
            var verzenList = verzen.ToList();
            var resultaat = _database.ZoekSpecifiek(setNaam, zoekNaam, verzenList, settings);
            if (resultaat.Status != LiturgieOplossingResultaat.Opgelost)
                return resultaat.Status;

            if (resultaat.Onderdeel.OrigineleNaam == FileEngineDefaults.CommonFilesSetName)
            {
                regel.DisplayEdit.Naam = resultaat.Fragment.OrigineleNaam;
                regel.DisplayEdit.VersenGebruikDefault = new VersenDefault(string.Empty);  // Expliciet: Common bestanden hebben nooit versen
            }
            else {
                regel.DisplayEdit.Naam = resultaat.Onderdeel.OrigineleNaam;
                regel.DisplayEdit.SubNaam = resultaat.Fragment.OrigineleNaam;
            }
            regel.Content = resultaat.Content.ToList();
            if (resultaat.ZonderContentSplitsing)
                regel.DisplayEdit.VersenGebruikDefault = new VersenDefault(string.Empty);  // Altijd default gebruiken omdat er altijd maar 1 content is
            regel.DisplayEdit.VolledigeContent = !verzenList.Any();

            // Basis waarde van tonen in overzicht bepalen (kan nog overschreven worden door de regel specifieke opties)
            var nietTonenInOverzicht = setNaam == FileEngineDefaults.CommonFilesSetName || (resultaat.StandaardNietTonenInLiturgie ?? false);
            regel.TonenInOverzicht = !nietTonenInOverzicht;

            // bepaal de naamgeving
            if (!IsNullOrWhiteSpace(resultaat.Onderdeel.DisplayNaam))
                regel.DisplayEdit.Naam = resultaat.Onderdeel.DisplayNaam.Equals(_defaultSetNameEmpty, StringComparison.CurrentCultureIgnoreCase) ? null : resultaat.Onderdeel.DisplayNaam;

            return null;
        }
        private LiturgieOplossingResultaat? BijbeltekstAanvuller(Regel regel, string setNaam, IEnumerable<ILiturgieInterpretatieBijbeltekstDeel> versDelen, ILiturgieSettings settings)
        {
            regel.VerwerkenAlsType = VerwerkingType.bijbeltekst;
            var content = new List<ILiturgieContent>();
            var versDelenLijst = versDelen.ToList();
            foreach(var deel in versDelenLijst)
            {
                var resultaat = _database.ZoekSpecifiek(VerwerkingType.bijbeltekst, setNaam, deel.Deel, deel.Verzen, settings);
                if (resultaat.Status != LiturgieOplossingResultaat.Opgelost)
                    return resultaat.Status;
                content.AddRange(resultaat.Content);
                // let op, naamgeving wordt buitenom geregeld
            }
            regel.Content = content.ToList();
            regel.DisplayEdit.VolledigeContent = versDelenLijst.Count == 1 && !versDelen.FirstOrDefault().Verzen.Any();
            return null;
        }

        public IEnumerable<ILiturgieOplossing> LosOp(IEnumerable<ILiturgieInterpretatie> items, IEnumerable<ILiturgieMapmaskArg> masks, ILiturgieSettings settings)
        {
            return items.Select(i => LosOp(i, masks, settings)).ToList();
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
            var veiligeZoekTekst = (zoekTekst ?? "").TrimStart();
            var veranderingGemaakt = vorigResultaat == null;
            var zoekRestricties = new ZoekRestricties(alsBijbeltekst);
            var aanname = vorigResultaat?.Aanname;
            var laatsteZoektekenIsFragmentWissel = veiligeZoekTekst.Length > 0 ? LiturgieInterpretator.InterpreteerLiturgieRuw.BenamingDeelScheidingstekens.Contains(veiligeZoekTekst.Last()) : false;

            var onderdeelLijst = KrijgBasisDatabaseLijst(zoekRestricties, true);
            var fragmentLijst = Enumerable.Empty<IVrijZoekresultaatMogelijkheid>();
            var vorigeZoektermSplit = _liturgieInterperator.VanTekstregel(vorigResultaat == null ? "" : vorigResultaat.ZoekTerm);
            var huidigeZoektermSplit = _liturgieInterperator.VanTekstregel(veiligeZoekTekst);

            // Wisselen tussen bijbeltekst vinkje of niet geeft natuurlijk grote wijziging
            if (vorigResultaat != null && vorigResultaat.AlsBijbeltekst != alsBijbeltekst)
            {
                veranderingGemaakt = true;
            }

            // Let op: Omdat de UI zelf filtert detecteren we hier alleen overgangen.

            // Kijk of er in de zoektekst een spatie is gebruikt, dan komt er nu een overgang aan
            if ((veiligeZoekTekst.Length > 0 && laatsteZoektekenIsFragmentWissel) || (string.IsNullOrWhiteSpace(vorigeZoektermSplit.Deel) && !string.IsNullOrWhiteSpace(huidigeZoektermSplit.Deel)))
            {
                // Fragment is er bij gekomen
                veranderingGemaakt = true;
                fragmentLijst = ZoekVerdieping(huidigeZoektermSplit.Benaming).Select(t => new ZoekresultaatItem()
                {
                    Weergave = $"{huidigeZoektermSplit.Benaming} {t.Resultaat}",
                    UitDatabase = t.Database.Weergave,
                }).ToList();

                // Geen aannames meer
                aanname = null;
            }
            // Zo gauw je de spatie weghaald is de overgang weer weg
            else if ((veiligeZoekTekst.Length == 0 || !laatsteZoektekenIsFragmentWissel) && string.IsNullOrWhiteSpace(huidigeZoektermSplit.Deel) && (vorigResultaat != null && vorigResultaat.ZoekTerm.Length > 0 && LiturgieInterpretator.InterpreteerLiturgieRuw.BenamingDeelScheidingstekens.Contains(vorigResultaat.ZoekTerm.Last())))
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
                fragmentLijst = ZoekVerdieping(aanname).Select(t => new ZoekresultaatItem()
                {
                    Weergave = $"{aanname} {t.Resultaat}",
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
        private IList<IZoekresultaat> ZoekBasisDatabaseLijst(ZoekRestricties zoekRestricties)
        {
            var alleDatabases = Enumerable.Empty<IZoekresultaat>();

            // zoekrestricties toepassen
            if (zoekRestricties.ZoekInBijbel && !zoekRestricties.ZoekInLiederen)
                alleDatabases = _database.ZoekGeneriekOnderdeelBijbel();
            else if (!zoekRestricties.ZoekInBijbel && zoekRestricties.ZoekInLiederen)
                alleDatabases = _database.ZoekGeneriekOnderdeelDefault();
            else if (zoekRestricties.ZoekInBijbel && zoekRestricties.ZoekInLiederen)
                alleDatabases = _database.ZoekGeneriekAlleOnderdelen();

            // Alle slide templates zoals amen, votum, bidden etc)
            if (zoekRestricties.ZoekInCommon)
                alleDatabases = alleDatabases.Concat(ZoekVerdieping(FileEngineDefaults.CommonFilesSetName));  

            return alleDatabases.ToList();
        }
        private IEnumerable<IZoekresultaat> ZoekVerdieping(string vanOnderdeelNaam)
        {
            return _database.ZoekGeneriekAlleFragmenten(vanOnderdeelNaam).ToList();
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

        public ILiturgieOptiesGebruiker ZoekStandaardOptiesUitZoekresultaat(string invoerTekst, IVrijZoekresultaat zoekresultaat)
        {
            if (string.IsNullOrWhiteSpace(invoerTekst))
                return null;
            var databaseNaam = string.Empty;
            if (zoekresultaat != null)
            {
                var invoerTekstSplitsing = _liturgieInterperator.VanTekstregel(invoerTekst);
                var teZoekenTekst = $"{invoerTekstSplitsing.Benaming} {invoerTekstSplitsing.Deel}";
                var itemInZoeklijst = zoekresultaat.AlleMogelijkheden.FirstOrDefault(z => z.Weergave == teZoekenTekst);
                if (itemInZoeklijst != null)
                    databaseNaam = itemInZoeklijst.UitDatabase;
                if (string.IsNullOrWhiteSpace(databaseNaam))
                    databaseNaam = zoekresultaat.VermoedelijkeDatabase;
            }
            return _liturgieInterperator.BepaalBasisOptiesTekstinvoer(invoerTekst, databaseNaam);
        }

        public ILiturgieOptiesGebruiker ToonOpties(string optiesInTekst)
        {
            return _liturgieInterperator.BepaalOptiesTekstinvoer(optiesInTekst);
        }


        public string MaakTotTekst(string invoerTekst, ILiturgieOptiesGebruiker opties, IVrijZoekresultaat zoekresultaat)
        {
            var tekstUitOpties = _liturgieInterperator.MaakTekstVanOpties(opties);
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
            return _liturgieInterperator.SplitsVoorOpties(liturgieRegel);
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

            public bool Equals(IVrijZoekresultaatMogelijkheid x, IVrijZoekresultaatMogelijkheid y)
            {
                if (x == null || y == null)
                    return false;
                return x.Weergave == y.Weergave;  // Alleen sorteren op weergave naam
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
