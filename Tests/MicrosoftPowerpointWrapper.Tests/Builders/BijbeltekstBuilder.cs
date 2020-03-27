using Generator.LiturgieInterpretator.Models;
using System.Collections.Generic;
using System.Linq;
using static mppt.RegelVerwerking.TekstVerdelerBijbeltekst;

namespace MicrosoftPowerpointWrapper.Tests.Builders
{
    public class BijbeltekstBuilder
    {
        public const int MaxRowLength = 30;
        private const string SimpleWord = "aaaaa";

        public IEnumerable<TekstBlok> Tekst { get; }
        public ILengteBerekenaar LengteBerekenaar => new LengteBerekenaarFaked();
        private List<Regel> _blokRegels = new List<Regel>();

        public BijbeltekstBuilder()
        {
            Tekst = new List<TekstBlok>() { new TekstBlok { Regels = _blokRegels } };
        }

        public BijbeltekstBuilder VoegVerzenToe(int aantal, int versLengte)
        {
            var vers = string.Join(" ", Enumerable.Range(0, (versLengte / SimpleWord.Length) + 1).Select(_ => SimpleWord));
            vers = vers.Substring(0, versLengte - 1).Trim() + ".";

            _blokRegels.AddRange(Enumerable.Range(0, aantal).Select(nr => new Regel {
                Nummer = nr + 1,
                Tekst = vers
            }));

            return this;
        }

        private class LengteBerekenaarFaked : ILengteBerekenaar
        {
            public float VerbruiktPercentageVanRegel(string tekst, bool needsPad)
            {
                return (float)(tekst.Length + (needsPad ? 1 : 0)) * 100 / MaxRowLength;
            }
        }
    }
}
