// Copyright 2018 door Erik de Roos
using ILiturgieDatabase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Generator.Tests
{
    public class InterpreteerLiturgieRuwTests
    {
        private ILiturgieInterpreteer _sut;

        [TestInitialize]
        public void Initialise()
        {
            _sut = new LiturgieInterpretator.InterpreteerLiturgieRuw();
        }

        [TestClass]
        public class VanTekstregelMethod : InterpreteerLiturgieRuwTests
        {
            [DataTestMethod]
            [DataRow("Deel1 Deel2: Deel3 (als:bijbeltekst)", "", new[] { "Deel1", "Deel2", "Deel3"} )]
            public void Splitsing_Basis(string input, string empty, string[] resultaten)
            {
                var benaming = resultaten[0];
                var deel = resultaten[1];
                var verzen = resultaten[2];
                const bool alsBijbeltekst = true;

                var resultaat = _sut.VanTekstregel(input);

                Assert.AreEqual(resultaat.Benaming, benaming);
                Assert.AreEqual(resultaat.Deel, deel);
                Assert.AreEqual(resultaat.VerzenZoalsIngevoerd, verzen);
                Assert.AreEqual(resultaat.OptiesGebruiker.AlsBijbeltekst, alsBijbeltekst);
            }

            [DataTestMethod]
            [DataRow("johannes 3: 5 (als:bijbeltekst)")]
            public void AlsBijbeltekst_IsBijbeltekst(string input)
            {
                var resultaat = _sut.VanTekstregel(input);

                Assert.IsInstanceOfType(resultaat, typeof(ILiturgieInterpretatieBijbeltekst));
            }

            [DataTestMethod]
            [DataRow("johannes 3: 5, 6 - 8: 3, 9: 3 (als:bijbeltekst)", "johannes", new[] { "3: 5, 6 -", "8: - 3,", "9: 3" })]
            [DataRow("johannes 3:5,6-8:3,9:3 (als:bijbeltekst)", "johannes", new[] { "3: 5,6-", "8: - 3,", "9: 3" })]
            public void AlsBijbeltekst_SplitsingMetEnZonderSpaties(string input, string resultaatBenaming, string[] resultaatDelen)
            {
                var resultaat = _sut.VanTekstregel(input) as ILiturgieInterpretatieBijbeltekst;
                Assert.IsNotNull(resultaat);

                var alleDelenGenummerd = resultaat.PerDeelVersen.Select((deelEnVers, i) => new { deelEnVers, i }).ToList();

                Assert.AreEqual(resultaat.Benaming, resultaatBenaming);
                Assert.AreEqual(alleDelenGenummerd.Count(), resultaatDelen.Count());
                alleDelenGenummerd.ForEach((a) => Assert.AreEqual(a.deelEnVers.ToString(), resultaatDelen[a.i]));
            }

            [DataTestMethod]
            [DataRow("johannes 3: 5, 8: 1, 9: 3 (als:bijbeltekst)", "johannes", new[] { "3: 5,", "8: 1,", "9: 3" })]
            [DataRow("johannes 3: 5, 7 - 8: 3 (als:bijbeltekst)", "johannes", new[] { "3: 5, 7 -", "8: - 3" })]
            public void AlsBijbeltekst_Splitsing(string input, string resultaatBenaming, string[] resultaatDelen)
            {
                var resultaat = _sut.VanTekstregel(input) as ILiturgieInterpretatieBijbeltekst;
                Assert.IsNotNull(resultaat);

                var alleDelenGenummerd = resultaat.PerDeelVersen.Select((deelEnVers, i) => new { deelEnVers, i }).ToList();

                Assert.AreEqual(resultaat.Benaming, resultaatBenaming);
                Assert.AreEqual(alleDelenGenummerd.Count(), resultaatDelen.Count());
                alleDelenGenummerd.ForEach((a) => Assert.AreEqual(a.deelEnVers.ToString(), resultaatDelen[a.i]));
            }
        }
    }
}
