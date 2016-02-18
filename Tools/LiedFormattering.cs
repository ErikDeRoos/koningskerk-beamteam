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
            if (string.IsNullOrWhiteSpace(regel.Display.SubNaam))
                return regel.Display.Naam;
            if ((regel.Content == null || regel.Content.Count() <= 1 || vanafDeelHint == null) && string.IsNullOrWhiteSpace(regel.Display.VersenDefault))
                return $"{regel.Display.Naam} {regel.Display.SubNaam}";
            if (!regel.Display.VersenAfleiden || regel.Content == null)
                return $"{regel.Display.Naam} {regel.Display.SubNaam}: {LiedVerzen(regel.Display, vanafDeelHint != null)}";
            var vanafDeel = vanafDeelHint ?? regel.Content.FirstOrDefault();  // Bij een deel hint tonen we alleen nog de huidige en komende versen
            var gebruikDeelRegels = regel.Content.SkipWhile(r => r != vanafDeel);
            return $"{regel.Display.Naam} {regel.Display.SubNaam}: {LiedVerzen(regel.Display, vanafDeelHint != null, gebruikDeelRegels)}";
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
            if (!regelDisplay.VersenAfleiden || vanDelen == null)
                return regelDisplay.VersenDefault ?? string.Empty;
            var over = vanDelen.Where(v => v.Nummer.HasValue).Select(v => v.Nummer.Value).ToList();
            if (!over.Any())
                return "";
            var builder = new StringBuilder(" ");
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
                    builder.Append(nieuweReeks[0]).Append(", ");
                else if (nieuweReeks.Count == 2)
                    builder.Append(string.Join(", ", nieuweReeks));
                else
                    builder.AppendFormat("{0} - {1}, ", nieuweReeks.First(), nieuweReeks.Last());
            }
            return builder.ToString().TrimEnd(',', ' ');
        }
    }
}
