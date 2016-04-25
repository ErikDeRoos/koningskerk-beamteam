//// Copyright 2016 door Erik de Roos
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace mppt.RegelVerwerking
//{
//    // TODO remote generator werkt niet meer met bijbeltekst want verzenden houd geen rekening met ILiturgieRegel van ander type
//    class VerwerkerBijbeltekst : IVerwerkFactory
//    {
//        public IVerwerk Init(IMppPresentatie toevoegenAanPresentatie)
//        {
//            return new Verwerker();
//        }

//        private class Verwerker : IVerwerk
//        {
//            public IVerwerkResultaat Verwerk(ILiturgieRegel regel, ILiturgieRegel volgende)
//            {
//                // Per onderdeel in de regel moet een sheet komen
//                foreach (var inhoud in regel.Content)
//                {
//                    // TODO bij regel type bijbeltekst eigen template vuller die teksten met nummering plaatst

//                    if (inhoud.InhoudType == InhoudType.Tekst)
//                        InvullenTekstOpTemplate(regel, inhoud, volgende);
//                    else
//                        ToevoegenSlides(regel, inhoud, volgende);
//                    if (_stop)
//                        break;
//                }
//            }

//            private SlideVuller InvullenBijbeltekst(string tempinhoud)
//            {
//                var returnValue = new SlideVuller();
//                var regels = SplitRegels.Split(tempinhoud);

//                // We moeten goed opletten bij het invullen van een liedtekst op een slide:
//                // -Het mogen niet te veel regels zijn (instellingen beperken dat)
//                // -We willen niet beginregels verspillen aan witruimte
//                // -Tussenwitregels willen we wel respecteren
//                // -Als we afbreken in een aaneengesloten stuk tekst moeten we kijken of we toch niet
//                //  naar een voorgaande witruimte kunnen afbreken

//                // kijk waar we gaan beginnen. Sla begin witregels over
//                var beginIndex = regels.Select((r, i) => new { Regel = r, Index = i })
//                  .Where(r => !SkipRegel(r.Regel))
//                  .Select(r => (int?)r.Index)  // nullable int zodat als we niets vinden we dat weten
//                  .FirstOrDefault();
//                if (!beginIndex.HasValue)
//                    return returnValue;  // er is niets over

//                // kijk waar we eindigen als we instellinge-aantal tellen vanaf ons startpunt
//                var eindIndex = regels.Select((r, i) => new { Regel = r, Index = i })
//                  .Where(r => r.Index >= beginIndex && (r.Index - beginIndex) < _buildDefaults.RegelsPerLiedSlide && r.Regel != NieuweSlideAanduiding)
//                  .Select(r => r.Index)  // eindindex is er altijd als er een begin is
//                  .LastOrDefault();

//                var optimaliseerEindIndex = eindIndex;
//                // Kijk of we niet beter op een eerdere witregel kunnen stoppen
//                if (!SkipRegel(regels[optimaliseerEindIndex]) && regels.Length != optimaliseerEindIndex + 1)
//                {
//                    var tryOptimaliseerEindIndex = regels.Select((r, i) => new { Regel = r, Index = i })
//                      .Skip(beginIndex.Value).Take(optimaliseerEindIndex + 1 - beginIndex.Value)
//                      .OrderByDescending(r => r.Index)
//                      .Where(r => SkipRegel(r.Regel))
//                      .Select(r => (int?)r.Index)
//                      .FirstOrDefault();
//                    if (tryOptimaliseerEindIndex.HasValue && tryOptimaliseerEindIndex.Value > beginIndex.Value)
//                        optimaliseerEindIndex = tryOptimaliseerEindIndex.Value;
//                }

//                // haal regels van het vers op
//                var insertLines = regels
//                  .Skip(beginIndex.Value).Take(optimaliseerEindIndex + 1 - beginIndex.Value)
//                  .Select(r => (r ?? "").Trim()).ToList();

//                // plaats de in te voegen regels in het tekstveld (geen enter aan het einde)
//                returnValue.Invullen = string.Join("", insertLines.Select((l, i) => l + (i + 1 == insertLines.Count ? "" : "\r\n")));

//                var overStart = optimaliseerEindIndex + 1;
//                if (overStart >= regels.Length)
//                    return returnValue;
//                if (regels[overStart] == NieuweSlideAanduiding)
//                    overStart++;
//                var overLines = regels.Skip(overStart).ToList();

//                // afbreek teken tonen alleen als een vers doormidden gebroken is
//                if (!SkipRegel(insertLines.Last()) && overLines.Any() && !SkipRegel(overLines.First()))
//                    returnValue.Invullen += "\r\n >>";

//                // Geef de resterende regels terug
//                returnValue.Over = string.Join("", overLines.Select((l, i) => l + (i + 1 == overLines.Count ? "" : "\r\n")));
//                return returnValue;
//            }
//        }
//    }
//}
