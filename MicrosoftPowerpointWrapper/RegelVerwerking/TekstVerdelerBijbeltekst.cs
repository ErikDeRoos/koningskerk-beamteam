// Copyright 2020 door Erik de Roos
using Generator.LiturgieInterpretator.Models;
using Generator.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mppt.RegelVerwerking
{
    public class TekstVerdelerBijbeltekst
    {
        /// <summary>
        /// Vul de bijbeltekst over meerdere slides
        /// </summary>
        public static IEnumerable<SlideData> OpdelenPerSlide(IEnumerable<TekstBlok> tekst, int regelsPerSlide, ILengteBerekenaar lengteBerekenaar, bool versOnderbrekingOverSlidesHeen)
        {
            // We moeten goed opletten bij het invullen van een liedtekst op een slide:
            // -Het mogen niet te veel regels zijn (instellingen beperken dat)
            // -Tussenwitregels willen we respecteren
            // -Als we afbreken binnen een bijbeltekst moeten we kijken of we toch niet
            //  naar een voorgaande witruimte kunnen afbreken
            var verzameldeRegels = new List<string>();
            var laatsteTekstregel = string.Empty;
            foreach (var blok in tekst)
            {
                var blokRegels = blok.Regels.ToList();
                var slidePasteNietVorigeKeer = false;

                // Voeg de tekst uit dit blok toe aan de slide en maak waar nodig nieuwe slides.
                int regelNr = 0;
                while (regelNr < blokRegels.Count)
                {
                    var regel = blokRegels[regelNr];
                    var regelWoorden = KnipInWoorden(regel).ToList();
                    var toevoegen = TryAdd(laatsteTekstregel, regelWoorden, lengteBerekenaar, verzameldeRegels.Count, regelsPerSlide);

                    // Als alles past, volg de normale eenvoudige flow
                    if (!toevoegen.SlideOverloop)
                    {
                        verzameldeRegels.AddRange(toevoegen.AddRows);
                        laatsteTekstregel = toevoegen.Over;
                        slidePasteNietVorigeKeer = false;

                        // Alles is gelukt, volgende regel
                        regelNr++;
                        continue;
                    }

                    // Het past niet (meer) op deze slide, en onderbreking over slides heen is niet toegestaan
                    // Schijf nog wel de overloop van de vorige keer weg, en ga dan opnieuw met de huidige regel aan de slag maar dan op een nieuwe slide
                    // Maar als dit net ook al gebeurde, toch maar overloop activeren wat als je gewoon een te lange tekst hebt gaat t nooit werken.
                    var tijdelijkOverloopToestaan = !versOnderbrekingOverSlidesHeen && slidePasteNietVorigeKeer;
                    if (!versOnderbrekingOverSlidesHeen && !tijdelijkOverloopToestaan)
                    {
                        if (laatsteTekstregel.Length > 0)
                        {
                            verzameldeRegels.Add(laatsteTekstregel);
                            laatsteTekstregel = string.Empty;
                        }
                        if (verzameldeRegels.Any())
                        {
                            yield return new SlideData() { Regels = verzameldeRegels.Select(r => r.Trim()).ToList() };
                            verzameldeRegels = new List<string>();
                        }

                        slidePasteNietVorigeKeer = true;
                        
                        // Redo de huidige regel
                        continue;
                    }

                    // Afhankelijk van de settings mogen teksten over slides heenlopen.
                    // Ga net zolang door met deze regel uitschrijven op slides totdat we op een slide komen waarop wel weer ruimte is.
                    do
                    {
                        // Alles wat op de slide paste uitsturen als slide
                        verzameldeRegels.AddRange(toevoegen.AddRows);
                        yield return new SlideData() { Regels = verzameldeRegels.Select(r => r.Trim()).ToList() };
                        verzameldeRegels = new List<string>();
                        laatsteTekstregel = string.Empty;

                        // Kijken of de overgebleven woorden passen
                        regelWoorden = toevoegen.OverloopWoorden.ToList();
                        toevoegen = TryAdd(laatsteTekstregel, regelWoorden, lengteBerekenaar, verzameldeRegels.Count, regelsPerSlide);
                    }
                    while (toevoegen.SlideOverloop);

                    // Oke, alles staat er op. Nu weer normale flow
                    verzameldeRegels.AddRange(toevoegen.AddRows);
                    laatsteTekstregel = toevoegen.Over;
                    slidePasteNietVorigeKeer = false;
                    regelNr++;
                }
                // Laatste restje van het blok nog toevoegen
                if (laatsteTekstregel.Length > 0)
                {
                    verzameldeRegels.Add(laatsteTekstregel);
                    laatsteTekstregel = string.Empty;
                }

                // Blok einde. Check of een witregel nog past.
                if (verzameldeRegels.Count + 1 >= regelsPerSlide)
                {
                    yield return new SlideData() { Regels = verzameldeRegels.Select(r => r.Trim()).ToList() };
                    verzameldeRegels = new List<string>();
                }
                else
                {
                    verzameldeRegels.Add(string.Empty);
                }
            }
            // Laatste restje verzamelde regels naar een nieuwe slide sturen
            if (verzameldeRegels.Any())
                yield return new SlideData() { Regels = verzameldeRegels.Select(r => r.Trim()).ToList() };
        }

        private static AddReturnValue TryAdd(string nogOver, List<string> regelWoorden, ILengteBerekenaar lengteBerekenaar, int slideRegelCount, int maxRegelsPerSlide)
        {
            var builder = new StringBuilder(nogOver);
            if (builder.Length > 0)
                builder.Append(" ");
            var regelBuildPercentage = lengteBerekenaar.VerbruiktPercentageVanRegel(builder.ToString(), true);
            var verzameldeRegels = new List<string>();
            var regelWoordenInOverloop = new List<string>();
            for (int woordNr = 0; woordNr < regelWoorden.Count; woordNr++)
            {
                var woord = regelWoorden[woordNr];

                var woordVerbruikPercentage = lengteBerekenaar.VerbruiktPercentageVanRegel(woord, false);
                if (regelBuildPercentage + woordVerbruikPercentage < 100)
                {
                    regelBuildPercentage += woordVerbruikPercentage;
                    builder.Append(woord);
                    continue;
                }
                else if (regelBuildPercentage + lengteBerekenaar.VerbruiktPercentageVanRegel(woord.Trim(), true) < 100)
                {
                    regelBuildPercentage += woordVerbruikPercentage;
                    builder.Append(woord.Trim());
                    continue;
                }

                verzameldeRegels.Add(builder.ToString());
                slideRegelCount++;
                if (slideRegelCount >= maxRegelsPerSlide)
                {
                    regelWoordenInOverloop.AddRange(regelWoorden.Skip(woordNr));
                    builder = new StringBuilder();
                    break;
                }

                regelBuildPercentage = woordVerbruikPercentage;
                builder = new StringBuilder(woord);
            }
            return new AddReturnValue()
            {
                AddRows = verzameldeRegels,
                Over = builder.ToString(),
                SlideOverloop = regelWoordenInOverloop.Count > 0,
                OverloopWoorden = regelWoordenInOverloop
            };
        }

        private static IEnumerable<string> KnipInWoorden(Regel regel)
        {
            var firstMatch = true;
            foreach (var woord in SplitRegels.KnipInWoorden(regel.Tekst))
            {
                if (firstMatch)
                    yield return $"{regel.Nummer} {woord}";
                else
                    yield return woord;
                firstMatch = false;
            }
        }

        private class AddReturnValue
        {
            public IEnumerable<string> AddRows { get; set; }
            public string Over { get; set; }
            public bool SlideOverloop { get; set; }
            public IEnumerable<string> OverloopWoorden { get; set; }
        }

        public class SlideData
        {
            private const bool TekstForcerenPerRegel = false;

            public IEnumerable<string> Regels { get; set; }

            public override string ToString()
            {
                return TekstForcerenPerRegel ? string.Join("\r\n", Regels) : string.Join(" ", Regels);
            }
        }

        public class TekstBlok
        {
            public IEnumerable<Regel> Regels { get; set; }
        }

        public class Regel
        {
            public int Nummer { get; set; }
            public string Tekst { get; set; }
        }
    }
}
