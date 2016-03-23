using ILiturgieDatabase;
using NUnit.Framework;
using System.Linq;

namespace Generator.Tests
{
    public class InterpreteerLiturgieRuw
    {

        [Test]
        public void VanTekstregel_Splitsing_basis()
        {
            var benaming = "Deel1";
            var deel = "Deel2";
            var verzen = "Deel3";
            var optie1 = "Deel4";

            var input = $"{benaming} {deel} : {verzen} ({optie1})";
            var sut = (new LiturgieInterpretator.InterpreteerLiturgieRuw()) as ILiturgieInterpreteer;

            var resultaat = sut.VanTekstregel(input);

            Assert.That(resultaat.Benaming, Is.EqualTo(benaming));
            Assert.That(resultaat.Deel, Is.EqualTo(deel));
            Assert.That(resultaat.VerzenZoalsIngevoerd, Is.EqualTo(verzen));
            Assert.That(resultaat.Opties.First(), Is.EqualTo(optie1));
        }
    }
}
