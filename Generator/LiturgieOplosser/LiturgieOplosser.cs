// Copyright 2019 door Erik de Roos
using Generator.Database.FileSystem;
using ILiturgieDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.String;

namespace Generator.LiturgieOplosser
{
    /// <summary>
    /// Converteer opgegeven liturgie objecten naar slides door ze in de database op te zoeken.
    /// Voeg er de nodige aanpassingen aan toe als dit gevraagd wordt in de invoer (andere naam, etc).
    /// </summary>
    public class LiturgieOplosser : ILiturgieSlideMaker
    {
        private readonly ILiturgieDatabase.ILiturgieDatabase _database;
        private readonly string _defaultSetNameEmpty;
        private readonly ILiturgieTekstNaarObject _liturgieTekstNaarObject;

        public LiturgieOplosser(ILiturgieDatabase.ILiturgieDatabase database, ILiturgieTekstNaarObject liturgieTekstNaarObject, string defaultSetNameEmpty)
        {
            _database = database;
            _defaultSetNameEmpty = defaultSetNameEmpty;
            _liturgieTekstNaarObject = liturgieTekstNaarObject;
        }

        public ITekstNaarSlideConversieResultaat ConverteerNaarSlide(ILiturgieTekstObject tekstInput, LiturgieSettings settings, IEnumerable<LiturgieMapmaskArg> masks = null)
        {
            var regel = new Slide {DisplayEdit = new LiturgieDisplay()};

            // verwerk de opties
            regel.VerwerkenAlsSlide = !tekstInput.OptiesGebruiker.NietVerwerkenViaDatabase;
            regel.TonenInOverzicht = tekstInput.OptiesGebruiker.ToonInOverzicht ?? (tekstInput.OptiesGebruiker.AlsBijbeltekst ? settings.ToonBijbeltekstenInLiturgie : true);
            regel.TonenInVolgende = tekstInput.OptiesGebruiker.ToonInVolgende ?? true;

            // regel visualisatie default
            regel.DisplayEdit.Naam = tekstInput.Benaming;
            regel.DisplayEdit.SubNaam = tekstInput.Deel;
            regel.DisplayEdit.VersenGebruikDefault = new VersenDefault();

            // zoek de regels in de database en pak ook de naamgeving daar uit over
            if (regel.VerwerkenAlsSlide)
            {
                var fout = Aanvullen(regel, tekstInput, settings);
                if (fout.HasValue)
                    return new ConversieResultaat(fout.Value, tekstInput);
            } else
            {
                regel.VerwerkenAlsType = VerwerkingType.nietverwerken;
                regel.DisplayEdit.VersenGebruikDefault = new VersenDefault(tekstInput.VerzenZoalsIngevoerd);
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
            if (!IsNullOrWhiteSpace(tekstInput.TeTonenNaamOpOverzicht))
            {
                regel.DisplayEdit.NaamOverzicht = tekstInput.TeTonenNaamOpOverzicht;
                regel.DisplayEdit.SubNaam = null;
            }
            if (!IsNullOrWhiteSpace(tekstInput.OptiesGebruiker.AlternatieveNaamOverzicht))
            {
                regel.DisplayEdit.NaamOverzicht = tekstInput.OptiesGebruiker.AlternatieveNaamOverzicht;
                regel.DisplayEdit.SubNaam = null;
            }
            // kijk of de gebruiker opties nog iets zeggen over alternatieve naamgeving
            if (!IsNullOrWhiteSpace(tekstInput.TeTonenNaam))
            {
                regel.DisplayEdit.Naam = tekstInput.TeTonenNaam;
                regel.DisplayEdit.SubNaam = null;
            }
            if (!IsNullOrWhiteSpace(tekstInput.OptiesGebruiker.AlternatieveNaam))
            {
                regel.DisplayEdit.Naam = tekstInput.OptiesGebruiker.AlternatieveNaam;
                regel.DisplayEdit.SubNaam = null;
            }

            // geef de oplossing terug
            return new ConversieResultaat(DatabaseZoekStatus.Opgelost, tekstInput, regel);
        }

        private DatabaseZoekStatus? Aanvullen(Slide regel, ILiturgieTekstObject item, LiturgieSettings settings)
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
        private DatabaseZoekStatus? NormaleAanvuller(Slide regel, string setNaam, string zoekNaam, IEnumerable<string> verzen, LiturgieSettings settings)
        {
            regel.VerwerkenAlsType = VerwerkingType.normaal;
            var verzenList = verzen.ToList();
            var resultaat = _database.ZoekSpecifiekItem(VerwerkingType.normaal, setNaam, zoekNaam, verzenList, settings);
            if (resultaat.Status != DatabaseZoekStatus.Opgelost)
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
        private DatabaseZoekStatus? BijbeltekstAanvuller(Slide regel, string setNaam, IEnumerable<ILiturgieInterpretatieBijbeltekstDeel> versDelen, LiturgieSettings settings)
        {
            regel.VerwerkenAlsType = VerwerkingType.bijbeltekst;
            var content = new List<ILiturgieContent>();
            var versDelenLijst = versDelen.ToList();
            foreach(var deel in versDelenLijst)
            {
                var resultaat = _database.ZoekSpecifiekItem(VerwerkingType.bijbeltekst, setNaam, deel.Deel, deel.Verzen, settings);
                if (resultaat.Status != DatabaseZoekStatus.Opgelost)
                    return resultaat.Status;
                content.AddRange(resultaat.Content);
                // let op, naamgeving wordt buitenom geregeld
            }
            regel.Content = content.ToList();
            regel.DisplayEdit.VolledigeContent = versDelenLijst.Count == 1 && !versDelen.FirstOrDefault().Verzen.Any();
            return null;
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
        private class Slide : ISlideOpbouw
        {
            public ILiturgieDisplay Display => DisplayEdit;
            public LiturgieDisplay DisplayEdit;

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
        private class LiturgieDisplay : ILiturgieDisplay
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
    }
}
