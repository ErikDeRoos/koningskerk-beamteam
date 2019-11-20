// Copyright 2019 door Erik de Roos
using Generator.Database.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mppt.LiedPresentator
{
    public class LiedFormatter : ILiedFormatter
    {
        public LiedFormatResult Huidig(ISlideOpbouw regel, ILiturgieContent vanafDeel)
        {
            return ToonMetVerzenEnEersteLos(regel, vanafDeel);
        }

        /// Bepaal de naam die getoond gaat worden bij 'volgende'.
        /// Je hebt hier invloed op door bepaalde slides te laten overslaan (OverslaanInVolgende), 
        /// of door ze leeg te laten (!TonenInVolgende).
        public LiedFormatResult Volgende(IEnumerable<ISlideOpbouw> volgenden, int overslaan = 0)
        {
            var komendeSlides = (volgenden ?? Enumerable.Empty<ISlideOpbouw>()).Where(v => !v.OverslaanInVolgende).ToList();
            // Alleen volgende tonen als volgende er is
            if (komendeSlides.Any() && overslaan >= 0)
            {
                var volgende = komendeSlides.Skip(overslaan).FirstOrDefault();
                if (volgende != null && volgende.TonenInVolgende)
                    return ToonWaarNodigMetVerzen(volgende);
            }
            return null;
        }

        public LiedFormatResult Liturgie(ISlideOpbouw regel)
        {
            return ToonWaarNodigMetVerzen(regel);
        }

        private static LiedFormatResult ToonWaarNodigMetVerzen(ISlideOpbouw regel)
        {
            var result = new LiedFormatResult()
            {
                Naam = regel.Display.NaamOverzicht
            };
            if (!string.IsNullOrWhiteSpace(regel.Display.SubNaam))
                result.SubNaam = regel.Display.SubNaam;
            result.Verzen = LiedVerzen(regel.Display, false, regel.Content);
            return result;
        }

        private static LiedFormatResult ToonMetVerzenEnEersteLos(ISlideOpbouw regel, ILiturgieContent vanafDeelHint)
        {
            var result = new LiedFormatResult()
            {
                Naam = regel.Display.Naam
            };
            if (!string.IsNullOrWhiteSpace(regel.Display.SubNaam))
                result.SubNaam = regel.Display.SubNaam;
            if (regel.Content == null)
                result.Verzen = LiedVerzen(regel.Display, true);
            else
            {
                var vanafDeel = vanafDeelHint ?? regel.Content.FirstOrDefault();  // Bij een deel hint tonen we alleen nog de huidige en komende versen
                var gebruikDeelRegels = regel.Content.SkipWhile(r => r != vanafDeel);
                result.Verzen = LiedVerzen(regel.Display, true, gebruikDeelRegels);
            }
            return result;
        }

        /// <summary>
        /// Maak een mooie samenvatting van de opgegeven nummers
        /// </summary>
        /// Probeer de nummers samen te vatten door een bereik te tonen.
        /// Waar niet mogelijk toon daar gewoon komma gescheiden nummers.
        /// Als het in beeld is dan wordt de eerste in ieder geval los getoond.
        /// <remarks>
        /// </remarks>
        private static string LiedVerzen(ILiturgieDisplay regelDisplay, bool toonEersteLos, IEnumerable<ILiturgieContent> vanDelen = null)
        {
            if (regelDisplay.VersenGebruikDefault.Gebruik || vanDelen == null || (!toonEersteLos && regelDisplay.VolledigeContent))
                return !string.IsNullOrEmpty(regelDisplay.VersenGebruikDefault.Tekst) ? regelDisplay.VersenGebruikDefault.Tekst : null;
            var over = vanDelen.Where(v => v.Nummer.HasValue).Select(v => v.Nummer.Value).ToList();
            if (!over.Any())
                return null;
            var builder = new StringBuilder();
            if (toonEersteLos)
            {
                builder.Append(over.First()).Append(", ");
                over.RemoveAt(0);
            }
            while (over.Any())
            {
                var nieuweReeks = new List<int> { over.First() };
                over.RemoveAt(0);
                while (over.Any() && over[0] == nieuweReeks.Last() + 1)
                {
                    nieuweReeks.Add(over[0]);
                    over.RemoveAt(0);
                }
                if (nieuweReeks.Count == 1)
                    builder.Append(nieuweReeks[0]);
                else if (nieuweReeks.Count == 2)
                    builder.Append(string.Join(", ", nieuweReeks));
                else
                    builder.AppendFormat("{0} - {1}", nieuweReeks.First(), nieuweReeks.Last());
                builder.Append(", ");
            }
            return builder.ToString().TrimEnd(',', ' ');
        }
    }
}
