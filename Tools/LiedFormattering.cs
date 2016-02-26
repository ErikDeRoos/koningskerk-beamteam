using ILiturgieDatabase;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools
{
    public class LiedFormattering
    {
        public static string LiedNaam(ILiturgieRegel regel, ILiturgieContent vanafDeelHint = null)
        {
            var builder = new StringBuilder(regel.Display.Naam);
            if (string.IsNullOrWhiteSpace(regel.Display.SubNaam))
                return builder.ToString();
            builder.Append($" {regel.Display.SubNaam}");
            var subVerzen = string.Empty;
            if (regel.Content == null)
                subVerzen = LiedVerzen(regel.Display, vanafDeelHint != null);
            else
            {
                var vanafDeel = vanafDeelHint ?? regel.Content.FirstOrDefault();  // Bij een deel hint tonen we alleen nog de huidige en komende versen
                var gebruikDeelRegels = regel.Content.SkipWhile(r => r != vanafDeel);
                subVerzen = LiedVerzen(regel.Display, vanafDeelHint != null, gebruikDeelRegels);
            }
            if (!string.IsNullOrWhiteSpace(subVerzen))
                builder.Append($": {subVerzen}");
            return builder.ToString();
        }
        
        /// <summary>
        /// Maak een mooie samenvatting van de opgegeven nummers
        /// </summary>
        /// Probeer de nummers samen te vatten door een bereik te tonen.
        /// Waar niet mogelijk toon daar gewoon komma gescheiden nummers.
        /// Als het in beeld is dan wordt de eerste in ieder geval los getoond.
        /// <remarks>
        /// </remarks>
        public static string LiedVerzen(ILiturgieDisplay regelDisplay, bool inBeeld, IEnumerable<ILiturgieContent> vanDelen = null)
        {
            if (regelDisplay.VersenGebruikDefault != null || vanDelen == null || (!inBeeld && regelDisplay.VolledigeContent))
                return regelDisplay.VersenGebruikDefault ?? string.Empty;
            var over = vanDelen.Where(v => v.Nummer.HasValue).Select(v => v.Nummer.Value).ToList();
            if (!over.Any())
                return "";
            var builder = new StringBuilder();
            if (inBeeld)
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
