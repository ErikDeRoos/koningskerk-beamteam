// Copyright 2019 door Erik de Roos
using Generator.Database;
using Generator.Database.FileSystem;
using Generator.Database.Models;
using Generator.LiturgieInterpretator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.String;

namespace Generator.LiturgieInterpretator
{
    /// <summary>
    /// Converteer opgegeven liturgie objecten naar slides door ze in de database op te zoeken.
    /// Voeg er de nodige aanpassingen aan toe als dit gevraagd wordt in de invoer (andere naam, etc).
    /// </summary>
    public class LiturgieOplosser : ILiturgieSlideMaker
    {
        private readonly ILiturgieDatabase _database;
        private readonly string _defaultSetNameEmpty;
        private readonly ILiturgieTekstNaarObject _liturgieTekstNaarObject;

        public LiturgieOplosser(ILiturgieDatabase database, ILiturgieTekstNaarObject liturgieTekstNaarObject, string defaultSetNameEmpty)
        {
            _database = database;
            _defaultSetNameEmpty = defaultSetNameEmpty;
            _liturgieTekstNaarObject = liturgieTekstNaarObject;
        }

        public ITekstNaarSlideConversieResultaat ConverteerNaarSlide(ILiturgieTekstObject tekstInput, LiturgieSettings settings, IEnumerable<LiturgieMapmaskArg> masks = null)
        {
            var regel = (Slide)null;
            var isBlancoSlide = false;

            // zoek de regels in de database en pak ook de naamgeving daar uit over
            var dbResult = (DatabaseResultaat)null;
            if (!tekstInput.OptiesGebruiker.NietVerwerkenViaDatabase)
            {
                dbResult = Opzoeken(tekstInput, settings);
                // TODO blanco check 
                if (dbResult.Status != DatabaseZoekStatus.Opgelost)
                    return new ConversieResultaat(dbResult.Status, tekstInput);
                regel = new Slide(dbResult);
                isBlancoSlide = string.Compare(tekstInput.Benaming, LiturgieOptieSettings.BlancoSlide, true) == 0;
            }
            else
            {
                regel = new Slide(tekstInput.Benaming, tekstInput.Deel, tekstInput.VerzenZoalsIngevoerd);
            }

            // verwerk de opties
            regel.VerwerkenAlsSlide = !tekstInput.OptiesGebruiker.NietVerwerkenViaDatabase;
            regel.TonenInOverzicht = tekstInput.OptiesGebruiker.ToonInOverzicht ?? regel.TonenInOverzicht;
            regel.TonenInVolgende = tekstInput.OptiesGebruiker.ToonInVolgende ?? true;
            regel.OverslaanInVolgende = isBlancoSlide;

            // Check of er een mask is (mooiere naam)
            // Anders underscores als spaties tonen
            var maskCheck = masks?.FirstOrDefault(m => Compare(m.RealName, regel.DisplayEdit.Naam, true) == 0);
            if (maskCheck != null)
                regel.DisplayEdit.Naam = maskCheck.Name;
            else
                regel.DisplayEdit.Naam = (regel.DisplayEdit.Naam ?? "").Replace("_", " ");
            regel.DisplayEdit.SubNaam = (regel.DisplayEdit.SubNaam ?? "").Replace("_", " ");
            
            // regel visualisatie na bewerking
            if (IsNullOrEmpty(regel.DisplayEdit.NaamOverzicht))
                regel.DisplayEdit.NaamOverzicht = regel.DisplayEdit.Naam;
            
            // kijk of de opties nog iets zeggen over alternatieve naamgeving
            if (!IsNullOrWhiteSpace(tekstInput.OptiesGebruiker.AlternatieveNaamOverzicht))
            {
                regel.DisplayEdit.NaamOverzicht = tekstInput.OptiesGebruiker.AlternatieveNaamOverzicht;
                regel.DisplayEdit.SubNaam = null;
            }
            else if (!IsNullOrWhiteSpace(tekstInput.TeTonenNaamOpOverzicht))
            {
                regel.DisplayEdit.NaamOverzicht = tekstInput.TeTonenNaamOpOverzicht;
                regel.DisplayEdit.SubNaam = null;
            }
            // kijk of de opties nog iets zeggen over alternatieve naamgeving
            if (!IsNullOrWhiteSpace(tekstInput.OptiesGebruiker.AlternatieveNaam))
            {
                regel.DisplayEdit.Naam = tekstInput.OptiesGebruiker.AlternatieveNaam;
                regel.DisplayEdit.SubNaam = null;
            }
            else if (!IsNullOrWhiteSpace(tekstInput.TeTonenNaam))
            {
                regel.DisplayEdit.Naam = tekstInput.TeTonenNaam;
                regel.DisplayEdit.SubNaam = null;
            }

            // geef de oplossing terug
            return new ConversieResultaat(DatabaseZoekStatus.Opgelost, tekstInput, regel);
        }

        private DatabaseResultaat Opzoeken(ILiturgieTekstObject item, LiturgieSettings settings)
        {
            var setNaam = item.Benaming;
            if (item is ILiturgieInterpretatieBijbeltekst)
            {
                return BijbeltekstOpzoeken(setNaam, item as ILiturgieInterpretatieBijbeltekst, settings);
            }
            var zoekNaam = item.Deel;
            if (IsNullOrEmpty(item.Deel))
            {
                setNaam = FileEngineDefaults.CommonFilesSetName;
                zoekNaam = item.Benaming;
            }

            return NormaalOpzoeken(setNaam, zoekNaam, item.Verzen.ToList(), settings);
        }
        private DatabaseResultaat NormaalOpzoeken(string setNaam, string zoekNaam, IEnumerable<string> verzen, LiturgieSettings settings)
        {
            var resultaat = new DatabaseResultaat(VerwerkingType.normaal);

            var verzenList = verzen.ToList();
            var dbResult = _database.KrijgItem(VerwerkingType.normaal, setNaam, zoekNaam, verzenList, settings);
            if (dbResult.Status != DatabaseZoekStatus.Opgelost)
                return new DatabaseResultaat(dbResult.Status);

            if (dbResult.Onderdeel.Naam == FileEngineDefaults.CommonFilesSetName)
            {
                resultaat.DisplayEdit.Naam = dbResult.Fragment.Naam;
                resultaat.DisplayEdit.VersenGebruikDefault = string.Empty;  // Expliciet: Common bestanden hebben nooit versen
            }
            else {
                resultaat.DisplayEdit.Naam = dbResult.Onderdeel.Naam;
                resultaat.DisplayEdit.SubNaam = dbResult.Fragment.Naam;
            }
            resultaat.Content = dbResult.Content.ToList();
            if (dbResult.ZonderContentSplitsing)
                resultaat.DisplayEdit.VersenGebruikDefault = string.Empty;  // Altijd default gebruiken omdat er altijd maar 1 content is
            resultaat.DisplayEdit.VolledigeContent = !verzenList.Any();

            // Basis waarde van tonen in overzicht bepalen
            if (setNaam != FileEngineDefaults.CommonFilesSetName && !(dbResult.StandaardNietTonenInLiturgie ?? false))
                resultaat.TonenInOverzicht = true;  // Nullable. Alleen true als we het belangrijk vinden. Is default, kan overschreven worden.

            // bepaal de naamgeving
            if (!IsNullOrWhiteSpace(dbResult.Onderdeel.AlternatieveNaam))
                resultaat.DisplayEdit.Naam = dbResult.Onderdeel.AlternatieveNaam.Equals(_defaultSetNameEmpty, StringComparison.CurrentCultureIgnoreCase) ? null : dbResult.Onderdeel.AlternatieveNaam;

            return resultaat;
        }
        private DatabaseResultaat BijbeltekstOpzoeken(string setNaam, ILiturgieInterpretatieBijbeltekst item, LiturgieSettings settings)
        {
            var resultaat = new DatabaseResultaat(VerwerkingType.bijbeltekst);

            var content = new List<ILiturgieContent>();
            var versDelenLijst = item.PerDeelVersen.ToList();
            var displaySelected = false;
            foreach (var deel in versDelenLijst)
            {
                var dbResult = _database.KrijgItem(VerwerkingType.bijbeltekst, setNaam, deel.Deel, deel.Verzen, settings);
                if (dbResult.Status != DatabaseZoekStatus.Opgelost)
                    return new DatabaseResultaat(dbResult.Status);
                content.AddRange(dbResult.Content);

                if (!displaySelected)
                {
                    resultaat.DisplayEdit.Naam = !string.IsNullOrEmpty(dbResult.Onderdeel.AlternatieveNaam) ? dbResult.Onderdeel.AlternatieveNaam : dbResult.Onderdeel.Naam;
                    resultaat.DisplayEdit.SubNaam = dbResult.Fragment.Naam;
                    displaySelected = true;
                }
            }
            resultaat.DisplayEdit.VersenGebruikDefault = item.VerzenZoalsIngevoerd;
            resultaat.Content = content.ToList();
            resultaat.DisplayEdit.VolledigeContent = versDelenLijst.Count == 1 && !versDelenLijst.FirstOrDefault().Verzen.Any();
            resultaat.TonenInOverzicht = settings.ToonBijbeltekstenInLiturgie;

            return resultaat;
        }


        private class ConversieResultaat : ITekstNaarSlideConversieResultaat
        {
            public DatabaseZoekStatus ResultaatStatus { get; }
            public ILiturgieTekstObject InputTekst { get; }
            public ISlideOpbouw ResultaatSlide { get; }

            public ConversieResultaat(DatabaseZoekStatus status, ILiturgieTekstObject invoerTekst, ISlideOpbouw regel = null)
            {
                ResultaatStatus = status;
                InputTekst = invoerTekst;
                ResultaatSlide = regel;
            }
        }
        private class DatabaseResultaat
        {
            public DatabaseZoekStatus Status { get; } = DatabaseZoekStatus.Opgelost;

            public DatabaseResultaat(VerwerkingType type) { VerwerkenAlsType = type; }
            public DatabaseResultaat(DatabaseZoekStatus status) { Status = status; }

            public LiturgieDisplayDb DisplayEdit { get; } = new LiturgieDisplayDb();

            public IEnumerable<ILiturgieContent> Content { get; set; }

            public bool? TonenInOverzicht { get; set; }
            public VerwerkingType VerwerkenAlsType { get; }
        }
        private class Slide : ISlideOpbouw
        {
            public ILiturgieDisplay Display => DisplayEdit;
            public LiturgieDisplay DisplayEdit { get; } = new LiturgieDisplay();

            public IEnumerable<ILiturgieContent> Content { get; set; }

            public bool TonenInOverzicht { get; set; }
            public bool VerwerkenAlsSlide { get; set; }
            public bool TonenInVolgende { get; set; }
            public bool OverslaanInVolgende { get; set; }
            public VerwerkingType VerwerkenAlsType { get; set; }

            public Slide (DatabaseResultaat opBasisVanDbResult)
            {
                DisplayEdit.Naam = opBasisVanDbResult.DisplayEdit.Naam;
                DisplayEdit.SubNaam = opBasisVanDbResult.DisplayEdit.SubNaam;
                DisplayEdit.VersenGebruikDefault = opBasisVanDbResult.DisplayEdit.VersenGebruikDefault ?? DisplayEdit.VersenGebruikDefault;
                DisplayEdit.VolledigeContent = opBasisVanDbResult.DisplayEdit.VolledigeContent;
                Content = opBasisVanDbResult.Content;
                TonenInOverzicht = opBasisVanDbResult.TonenInOverzicht ?? false;
                VerwerkenAlsType = opBasisVanDbResult.VerwerkenAlsType;
            }
            public Slide(string naam, string subnaam, string verzenZoalsIngevoerd)
            {
                DisplayEdit.Naam = naam;
                DisplayEdit.SubNaam = subnaam;
                DisplayEdit.VersenGebruikDefault = verzenZoalsIngevoerd;
                VerwerkenAlsType = VerwerkingType.nietverwerken;
            }

            public override string ToString()
            {
                return $"{DisplayEdit.Naam} {DisplayEdit.SubNaam}";
            }
        }
        private class LiturgieDisplay : ILiturgieDisplay
        {
            public string Naam { get; set; }
            public string NaamOverzicht { get; set; }
            public string SubNaam { get; set; }
            public bool VolledigeContent { get; set; }
            public string VersenGebruikDefault { get; set; }
        }
        private class LiturgieDisplayDb
        {
            public string Naam { get; set; }
            public string SubNaam { get; set; }
            public bool VolledigeContent { get; set; }
            public string VersenGebruikDefault { get; set; }
        }
    }
}
