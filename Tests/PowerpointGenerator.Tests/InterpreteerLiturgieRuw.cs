using ILiturgieDatabase;
using NUnit.Framework;
using System.Linq;

namespace Generator.Tests
{
    public class InterpreteerLiturgieRuw
    {

        [TestCase("Deel1 Deel2: Deel3 (Deel4)", new[] { "Deel1", "Deel2", "Deel3", "Deel4" })]
        public void VanTekstregel_Splitsing_Basis(string input, string[] resultaten)
        {
            var benaming = resultaten[0];
            var deel = resultaten[1];
            var verzen = resultaten[2];
            var optie1 = resultaten[3];
            var sut = (new LiturgieInterpretator.InterpreteerLiturgieRuw()) as ILiturgieInterpreteer;

            var resultaat = sut.VanTekstregel(input);

            Assert.That(resultaat.Benaming, Is.EqualTo(benaming));
            Assert.That(resultaat.Deel, Is.EqualTo(deel));
            Assert.That(resultaat.VerzenZoalsIngevoerd, Is.EqualTo(verzen));
            Assert.That(resultaat.Opties.First(), Is.EqualTo(optie1));
        }

        [TestCase("johannes 3: 5 (als:bijbeltekst)")]
        public void VanTekstregel_AlsBijbeltekst_IsBijbeltekst(string input)
        {
            var sut = (new LiturgieInterpretator.InterpreteerLiturgieRuw()) as ILiturgieInterpreteer;

            var resultaat = sut.VanTekstregel(input);

            Assert.That(resultaat, Is.AssignableTo<ILiturgieInterpretatieBijbeltekst>());
        }

        [TestCase("johannes 3: 5, 7, 9 - 8:1, 3, 9: 5 - 10 (als:bijbeltekst)", "johannes", new[] { "3: 5, 7, 9 -", "8:1, 3, ", "9: 5 - 10" })]
        public void VanTekstregel_AlsBijbeltekst_Splitsing(string input, string resultaatBenaming, string[] resultaatDelen)
        {
            var sut = (new LiturgieInterpretator.InterpreteerLiturgieRuw()) as ILiturgieInterpreteer;

            var resultaat = sut.VanTekstregel(input) as ILiturgieInterpretatieBijbeltekst;
            var alleDelenGenummerd = resultaat.PerDeelVersen.Select((deelEnVers, i) => new { deelEnVers, i }).ToList();

            Assert.That(resultaat.Benaming, Is.EqualTo(resultaatBenaming));
            Assert.That(alleDelenGenummerd.Count(), Is.EqualTo(resultaatDelen.Count()));
            alleDelenGenummerd.ForEach((a) => Assert.That(a.deelEnVers.ToString(), Is.EqualTo(resultaatDelen[a.i])));
        }
    }
}
