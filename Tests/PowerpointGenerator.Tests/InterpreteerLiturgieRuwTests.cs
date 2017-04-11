// Copyright 2016 door Erik de Roos
using ILiturgieDatabase;
using NUnit.Framework;
using System.Linq;

namespace Generator.Tests
{
    public class InterpreteerLiturgieRuwTests
    {
        ILiturgieInterpreteer sut;

        [OneTimeSetUp]
        public void Initialise()
        {
            sut = new LiturgieInterpretator.InterpreteerLiturgieRuw();
        }

        [TestFixture]
        public class VanTekstregelMethod : InterpreteerLiturgieRuwTests
        {
            [TestCase("Deel1 Deel2: Deel3 (als:bijbeltekst)", new[] { "Deel1", "Deel2", "Deel3" })]
            public void Splitsing_Basis(string input, string[] resultaten)
            {
                var benaming = resultaten[0];
                var deel = resultaten[1];
                var verzen = resultaten[2];
                var alsBijbeltekst = true;

                var resultaat = sut.VanTekstregel(input);

                Assert.That(resultaat.Benaming, Is.EqualTo(benaming));
                Assert.That(resultaat.Deel, Is.EqualTo(deel));
                Assert.That(resultaat.VerzenZoalsIngevoerd, Is.EqualTo(verzen));
                Assert.That(resultaat.OptiesGebruiker.AlsBijbeltekst, Is.EqualTo(alsBijbeltekst));
            }

            [TestCase("johannes 3: 5 (als:bijbeltekst)")]
            public void AlsBijbeltekst_IsBijbeltekst(string input)
            {
                var resultaat = sut.VanTekstregel(input);

                Assert.That(resultaat, Is.AssignableTo<ILiturgieInterpretatieBijbeltekst>());
            }

            [TestCase("johannes 3: 5, 6 - 8: 3, 9: 3 (als:bijbeltekst)", "johannes", new[] { "3: 5, 6 -", "8: - 3,", "9: 3" })]
            [TestCase("johannes 3:5,6-8:3,9:3 (als:bijbeltekst)", "johannes", new[] { "3: 5,6-", "8: - 3,", "9: 3" })]
            public void AlsBijbeltekst_SplitsingMetEnZonderSpaties(string input, string resultaatBenaming, string[] resultaatDelen)
            {
                var resultaat = sut.VanTekstregel(input) as ILiturgieInterpretatieBijbeltekst;
                var alleDelenGenummerd = resultaat.PerDeelVersen.Select((deelEnVers, i) => new { deelEnVers, i }).ToList();

                Assert.That(resultaat.Benaming, Is.EqualTo(resultaatBenaming));
                Assert.That(alleDelenGenummerd.Count(), Is.EqualTo(resultaatDelen.Count()));
                alleDelenGenummerd.ForEach((a) => Assert.That(a.deelEnVers.ToString(), Is.EqualTo(resultaatDelen[a.i])));
            }

            [TestCase("johannes 3: 5, 8: 1, 9: 3 (als:bijbeltekst)", "johannes", new[] { "3: 5,", "8: 1,", "9: 3" })]
            [TestCase("johannes 3: 5, 7 - 8: 3 (als:bijbeltekst)", "johannes", new[] { "3: 5, 7 -", "8: - 3" })]
            public void AlsBijbeltekst_Splitsing(string input, string resultaatBenaming, string[] resultaatDelen)
            {
                var resultaat = sut.VanTekstregel(input) as ILiturgieInterpretatieBijbeltekst;
                var alleDelenGenummerd = resultaat.PerDeelVersen.Select((deelEnVers, i) => new { deelEnVers, i }).ToList();

                Assert.That(resultaat.Benaming, Is.EqualTo(resultaatBenaming));
                Assert.That(alleDelenGenummerd.Count(), Is.EqualTo(resultaatDelen.Count()));
                alleDelenGenummerd.ForEach((a) => Assert.That(a.deelEnVers.ToString(), Is.EqualTo(resultaatDelen[a.i])));
            }
        }
    }
}
