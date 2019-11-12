// Copyright 2018 door Erik de Roos
using FakeItEasy;
using Generator.Database.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using mppt.LiedPresentator;
using System.Collections.Generic;
using System.Linq;

namespace MicrosoftPowerpointWrapper.Tests
{
    public class LiedFormatteringTests
    {
        private ISlideOpbouw _liturgieRegel;
        private ILiedFormatter _sut;

        [TestInitialize]
        public void Initialise()
        {
            _liturgieRegel = A.Fake<ISlideOpbouw>();
            _sut = new LiedFormatter();
        }

        [TestClass]
        public class LiturgieMethod : LiedFormatteringTests
        {
            [DataTestMethod]
            [DataRow("Psalm")]
            public void SpecifiekeLiturgieNaam_FormatteertStrak(string naam)
            {
                A.CallTo(() => _liturgieRegel.Display.NaamOverzicht).Returns(naam);

                var geformatteerd = _sut.Liturgie(_liturgieRegel);

                Assert.AreEqual(geformatteerd.Naam, naam);
            }

            [DataTestMethod]
            [DataRow("Psalm", "100", 21, 22)]
            public void NaamEnSubnaamEnVersMetVolledigeContent_FormatteertStrak(string naam, string subNaam, int versNummer1, int versNummer2)
            {
                A.CallTo(() => _liturgieRegel.Display.NaamOverzicht).Returns(naam);
                A.CallTo(() => _liturgieRegel.Display.SubNaam).Returns(subNaam);
                A.CallTo(() => _liturgieRegel.Display.VolledigeContent).Returns(true);
                var liturgieRegelContent1 = A.Fake<ILiturgieContent>();
                A.CallTo(() => liturgieRegelContent1.Nummer).Returns(versNummer1);
                var liturgieRegelContent2 = A.Fake<ILiturgieContent>();
                A.CallTo(() => liturgieRegelContent2.Nummer).Returns(versNummer2);
                A.CallTo(() => _liturgieRegel.Content).Returns(new List<ILiturgieContent>() { liturgieRegelContent1, liturgieRegelContent2 });

                var geformatteerd = _sut.Liturgie(_liturgieRegel);

                Assert.AreEqual(geformatteerd.Naam, naam);
                Assert.AreEqual(geformatteerd.SubNaam, subNaam);
                Assert.IsNull(geformatteerd.Verzen);
            }

            [DataTestMethod]
            [DataRow("10, 5")]
            public void ZonderDelen_ToontDefault(string defaultNaam)
            {
                A.CallTo(() => _liturgieRegel.Content).Returns(null);
                A.CallTo(() => _liturgieRegel.Display.VersenGebruikDefault.Tekst).Returns(defaultNaam);

                var geformatteerd = _sut.Liturgie(_liturgieRegel);

                Assert.AreEqual(geformatteerd.Verzen, defaultNaam);
            }

            [DataTestMethod]
            [DataRow(new [] { 1, 2, 3, 4 })]
            public void VolledigeContent_ToonLeeg(int[] aansluitendeNummerLijst)
            {
                A.CallTo(() => _liturgieRegel.Display.VolledigeContent).Returns(true);
                A.CallTo(() => _liturgieRegel.Content).Returns(PrepareContentLijst(aansluitendeNummerLijst).ToList());

                var geformatteerd = _sut.Liturgie(_liturgieRegel);

                Assert.IsNull(geformatteerd.Verzen);
            }

            [DataTestMethod]
            [DataRow(new [] { 1, 2, 3, 4 })]
            public void AansluitendeNummering_ToonGekoppeld(int[] aansluitendeNummerLijst)
            {
                A.CallTo(() => _liturgieRegel.Display.VolledigeContent).Returns(false);
                var nummerLijst = PrepareContentLijst(aansluitendeNummerLijst).ToList();
                A.CallTo(() => _liturgieRegel.Content).Returns(nummerLijst);

                var geformatteerd = _sut.Liturgie(_liturgieRegel);

                Assert.AreEqual(geformatteerd.Verzen, $"{nummerLijst.First().Nummer} - {nummerLijst.Last().Nummer}");
            }

        }

        [TestClass]
        public class HuidigMethod : LiedFormatteringTests
        {

            [DataTestMethod]
            [DataRow("Psalm")]
            public void AlleenNaam_FormatteertStrak(string naam)
            {
                A.CallTo(() => _liturgieRegel.Display.Naam).Returns(naam);

                var geformatteerd = _sut.Huidig(_liturgieRegel, null);

                Assert.AreEqual(geformatteerd.Naam, naam);
            }

            [DataTestMethod]
            [DataRow("Psalm", "100")]
            public void NaamEnSubnaam_FormatteertStrak(string naam, string subNaam)
            {
                A.CallTo(() => _liturgieRegel.Display.Naam).Returns(naam);
                A.CallTo(() => _liturgieRegel.Display.SubNaam).Returns(subNaam);

                var geformatteerd = _sut.Huidig(_liturgieRegel, null);

                Assert.AreEqual(geformatteerd.Naam, naam);
                Assert.AreEqual(geformatteerd.SubNaam, subNaam);
            }

            [DataTestMethod]
            [DataRow("Psalm", "100", 21)]
            public void NaamEnSubnaamEnVersMetDeelVanContent_FormatteertStrak(string naam, string subNaam, int versNummer1)
            {
                A.CallTo(() => _liturgieRegel.Display.Naam).Returns(naam);
                A.CallTo(() => _liturgieRegel.Display.SubNaam).Returns(subNaam);
                A.CallTo(() => _liturgieRegel.Display.VolledigeContent).Returns(false);
                var liturgieRegelContent1 = A.Fake<ILiturgieContent>();
                A.CallTo(() => liturgieRegelContent1.Nummer).Returns(versNummer1);
                A.CallTo(() => _liturgieRegel.Content).Returns(new List<ILiturgieContent>() { liturgieRegelContent1 });

                var geformatteerd = _sut.Huidig(_liturgieRegel, null);

                Assert.AreEqual(geformatteerd.Naam, naam);
                Assert.AreEqual(geformatteerd.SubNaam, subNaam);
                Assert.AreEqual(geformatteerd.Verzen, $"{versNummer1}");
            }

            [DataTestMethod]
            [DataRow("Psalm", "100", 21, 22)]
            public void NaamEnSubnaamEnVersMetVolledigeContent_FormatteertStrak(string naam, string subNaam, int versNummer1, int versNummer2)
            {
                A.CallTo(() => _liturgieRegel.Display.Naam).Returns(naam);
                A.CallTo(() => _liturgieRegel.Display.SubNaam).Returns(subNaam);
                A.CallTo(() => _liturgieRegel.Display.VolledigeContent).Returns(true);
                var liturgieRegelContent1 = A.Fake<ILiturgieContent>();
                A.CallTo(() => liturgieRegelContent1.Nummer).Returns(versNummer1);
                var liturgieRegelContent2 = A.Fake<ILiturgieContent>();
                A.CallTo(() => liturgieRegelContent2.Nummer).Returns(versNummer2);
                A.CallTo(() => _liturgieRegel.Content).Returns(new List<ILiturgieContent>() { liturgieRegelContent1, liturgieRegelContent2 });

                var geformatteerd = _sut.Huidig(_liturgieRegel, liturgieRegelContent1);

                Assert.AreEqual(geformatteerd.Naam, naam);
                Assert.AreEqual(geformatteerd.SubNaam, subNaam);
                Assert.AreEqual(geformatteerd.Verzen, $"{versNummer1}, {versNummer2}");
            }

            [DataTestMethod]
            [DataRow(new [] { 1, 2, 3, 4 })]
            public void VolledigeContent_ToonEersteLosToonRestGekoppeld(int[] aansluitendeNummerLijst)
            {
                A.CallTo(() => _liturgieRegel.Display.VolledigeContent).Returns(true);
                var nummerLijst = PrepareContentLijst(aansluitendeNummerLijst).ToList();
                A.CallTo(() => _liturgieRegel.Content).Returns(nummerLijst);

                var geformatteerd = _sut.Huidig(_liturgieRegel, nummerLijst.First());

                Assert.AreEqual(geformatteerd.Verzen, $"{nummerLijst.First().Nummer}, {nummerLijst.Skip(1).First().Nummer} - {nummerLijst.Last().Nummer}");
            }

            [DataTestMethod]
            [DataRow(new [] { 1, 2, 3, 4 })]
            public void AansluitendeNummering_ToonEersteLosToonRestGekoppeld(int[] aansluitendeNummerLijst)
            {
                A.CallTo(() => _liturgieRegel.Display.VolledigeContent).Returns(false);
                var nummerLijst = PrepareContentLijst(aansluitendeNummerLijst).ToList();
                A.CallTo(() => _liturgieRegel.Content).Returns(nummerLijst);

                var geformatteerd = _sut.Huidig(_liturgieRegel, nummerLijst.First());

                Assert.AreEqual(geformatteerd.Verzen, $"{nummerLijst.First().Nummer}, {nummerLijst.Skip(1).First().Nummer} - {nummerLijst.Last().Nummer}");
            }

            [DataTestMethod]
            [DataRow(new [] { 1, 2, 3, 4, 5, 7 })]
            public void AansluitendeNummeringMetLosNummer_ToonLosToonReeksToonLos(int[] aansluitendeNummerLijstMetLaasteLos)
            {
                A.CallTo(() => _liturgieRegel.Display.VolledigeContent).Returns(false);
                var nummerLijst = PrepareContentLijst(aansluitendeNummerLijstMetLaasteLos).ToList();
                A.CallTo(() => _liturgieRegel.Content).Returns(nummerLijst);

                var geformatteerd = _sut.Huidig(_liturgieRegel, nummerLijst.First());

                Assert.AreEqual(geformatteerd.Verzen, $"{nummerLijst.First().Nummer}, {nummerLijst.Skip(1).First().Nummer} - {nummerLijst.OrderByDescending(n => n.Nummer).Skip(1).First().Nummer}, {nummerLijst.Last().Nummer}");
            }

            [DataTestMethod]
            [DataRow(new [] { 5, 6, 7, 12, 14 })]
            public void AansluitendeNummeringMetLosNummer_ToonLos(int[] aansluitendeNummerLijstMetLaaste2Los)
            {
                A.CallTo(() => _liturgieRegel.Display.VolledigeContent).Returns(false);
                var nummerLijst = PrepareContentLijst(aansluitendeNummerLijstMetLaaste2Los).ToList();
                A.CallTo(() => _liturgieRegel.Content).Returns(nummerLijst);

                var geformatteerd = _sut.Huidig(_liturgieRegel, nummerLijst.First());

                Assert.AreEqual(geformatteerd.Verzen, string.Join(", ", nummerLijst.Select(n => n.Nummer)));
            }
        }

        private static IEnumerable<ILiturgieContent> PrepareContentLijst(int[] aansluitendeNummerLijst)
        {
            foreach (var n in aansluitendeNummerLijst)
            {
                var added = A.Fake<ILiturgieContent>();
                A.CallTo(() => added.Nummer).Returns(n);
                yield return added;
            }
        }
    }
}
