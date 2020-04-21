// Copyright 2020 door Erik de Roos
using Generator.Tools;
using System.Linq;

namespace mppt.RegelVerwerking
{
    public class TekstVerdelerLied
    {
        private const string NieuweSlideAanduiding = "#";
        private const string OnderbrekingAanduiding = " >>";

        /// <summary>
        /// Vul de liedtekst over meerdere slides
        /// </summary>
        public static SlideVuller InvullenLiedTekst(string tempinhoud, int regelsPerLiedSlide, bool visualiseerAfbreking)
        {
            var returnValue = new SlideVuller();
            var regels = SplitRegels.Split(tempinhoud);

            // We moeten goed opletten bij het invullen van een liedtekst op een slide:
            // -Het mogen niet te veel regels zijn (instellingen beperken dat)
            // -We willen niet beginregels verspillen aan witruimte
            // -Tussenwitregels willen we wel respecteren
            // -Als we afbreken in een aaneengesloten stuk tekst moeten we kijken of we toch niet
            //  naar een voorgaande witruimte kunnen afbreken

            // kijk waar we gaan beginnen. Sla begin witregels over
            var beginIndex = regels.Select((r, i) => new { Regel = r, Index = i })
              .Where(r => !SkipRegel(r.Regel))
              .Select(r => (int?)r.Index)  // nullable int zodat als we niets vinden we dat weten
              .FirstOrDefault();
            if (!beginIndex.HasValue)
                return returnValue;  // er is niets over

            // kijk waar we eindigen als we instellinge-aantal tellen vanaf ons startpunt
            var eindIndex = regels.Select((r, i) => new { Regel = r, Index = i })
              .Where(r => r.Index >= beginIndex && (r.Index - beginIndex) < regelsPerLiedSlide && r.Regel != NieuweSlideAanduiding)
              .Select(r => r.Index)  // eindindex is er altijd als er een begin is
              .LastOrDefault();

            var optimaliseerEindIndex = eindIndex;
            // Kijk of we niet beter op een eerdere witregel kunnen stoppen
            if (!SkipRegel(regels[optimaliseerEindIndex]) && regels.Length != optimaliseerEindIndex + 1)
            {
                var tryOptimaliseerEindIndex = regels.Select((r, i) => new { Regel = r, Index = i })
                  .Skip(beginIndex.Value).Take(optimaliseerEindIndex + 1 - beginIndex.Value)
                  .OrderByDescending(r => r.Index)
                  .Where(r => SkipRegel(r.Regel))
                  .Select(r => (int?)r.Index)
                  .FirstOrDefault();
                if (tryOptimaliseerEindIndex.HasValue && tryOptimaliseerEindIndex.Value > beginIndex.Value)
                    optimaliseerEindIndex = tryOptimaliseerEindIndex.Value;
            }

            // haal regels van het vers op
            var insertLines = regels
              .Skip(beginIndex.Value).Take(optimaliseerEindIndex + 1 - beginIndex.Value)
              .Select(r => (r ?? "").Trim()).ToList();

            // plaats de in te voegen regels in het tekstveld (geen enter aan het einde)
            returnValue.Invullen = string.Join("", insertLines.Select((l, i) => l + (i + 1 == insertLines.Count ? "" : "\r\n")));

            var overStart = optimaliseerEindIndex + 1;
            if (overStart >= regels.Length)
                return returnValue;
            if (regels[overStart] == NieuweSlideAanduiding)
                overStart++;
            var overLines = regels.Skip(overStart).ToList();

            // afbreek teken tonen alleen als een vers doormidden gebroken is
            if (visualiseerAfbreking && !SkipRegel(insertLines.Last()) && overLines.Any() && !SkipRegel(overLines.First()))
                returnValue.Invullen += $"\r\n{OnderbrekingAanduiding}";

            // Geef de resterende regels terug
            returnValue.Over = string.Join("", overLines.Select((l, i) => l + (i + 1 == overLines.Count ? "" : "\r\n")));
            return returnValue;
        }
        private static bool SkipRegel(string regel)
        {
            return string.IsNullOrWhiteSpace(regel) || regel == NieuweSlideAanduiding;
        }

        public class SlideVuller
        {
            public string Invullen { get; set; }
            public string Over { get; set; }
        }
    }
}
