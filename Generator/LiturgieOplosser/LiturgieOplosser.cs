// Copyright 2016 door Erik de Roos
using Generator.Database;
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
            regel.VerwerkenAlsSlide = !trimmedOpties.Any(o => o.StartsWith(LiturgieDatabaseSettings.OptieNietVerwerken, StringComparison.CurrentCultureIgnoreCase));
            regel.TonenInVolgende = !trimmedOpties.Any(o => o.StartsWith(LiturgieDatabaseSettings.OptieNietTonenInVolgende, StringComparison.CurrentCultureIgnoreCase));
            regel.TonenInOverzicht = !trimmedOpties.Any(o => o.StartsWith(LiturgieDatabaseSettings.OptieNietTonenInOverzicht, StringComparison.CurrentCultureIgnoreCase));

            // regel visualisatie default
            regel.DisplayEdit.Naam = item.Benaming;
            regel.DisplayEdit.SubNaam = item.Deel;
            regel.DisplayEdit.VersenGebruikDefault = new VersenDefault();

            // zoek de regels in de database en pak ook de naamgeving daar uit over
            if (regel.VerwerkenAlsSlide)
            {
                var setNaam = item.Benaming;
                var zoekNaam = item.Deel;
                if (IsNullOrEmpty(item.Deel))
                {
                    setNaam = FileEngineDefaults.CommonFilesSetName;
                    zoekNaam = item.Benaming;
                    regel.TonenInOverzicht = false;  // TODO tijdelijk default gedrag van het niet tonen van algemene items in het overzicht overgenomen uit de oude situatie
                }
                
                var fout = Aanvullen(regel, setNaam, zoekNaam, item.Verzen.ToList());
                if (fout.HasValue)
                    return new Oplossing(fout.Value, item);
            } else
            {
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
            var optieMetAltNaamOverzicht = GetOptieParam(trimmedOpties, LiturgieDatabaseSettings.OptieAlternatieveNaamOverzicht);
            if (!IsNullOrWhiteSpace(optieMetAltNaamOverzicht))
                regel.DisplayEdit.NaamOverzicht = optieMetAltNaamOverzicht;
            var optieMetAltNaamVolgende = GetOptieParam(trimmedOpties, LiturgieDatabaseSettings.OptieAlternatieveNaam);
            if (!IsNullOrWhiteSpace(optieMetAltNaamVolgende))
                regel.DisplayEdit.Naam = optieMetAltNaamOverzicht;

            // geef de oplossing terug
            return new Oplossing(LiturgieOplossingResultaat.Opgelost, item, regel);
        }

        private LiturgieOplossingResultaat? Aanvullen(Regel regel, string setNaam, string zoekNaam, IEnumerable<string> verzen)
        {
            var verzenList = verzen.ToList();
            var resultaat = _database.ZoekOnderdeel(setNaam, zoekNaam, verzenList);
            if (resultaat.Fout != null)
                return resultaat.Fout;

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

        private static string GetOptieParam(IEnumerable<string> opties, string optie)
        {
            var optieMetParam = opties.FirstOrDefault(o => o.StartsWith(optie, StringComparison.CurrentCultureIgnoreCase));
            return optieMetParam?.Substring(optie.Length).Trim();
        }

        public IEnumerable<ILiturgieOplossing> LosOp(IEnumerable<ILiturgieInterpretatie> items, IEnumerable<ILiturgieMapmaskArg> masks)
        {
            return items.Select(i => LosOp(i, masks)).ToList();
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
    }
}
