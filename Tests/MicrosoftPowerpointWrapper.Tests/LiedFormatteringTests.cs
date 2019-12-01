// Copyright 2019 door Erik de Roos
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftPowerpointWrapper.Tests.Builders;
using mppt.LiedPresentator;
using System.Linq;

namespace MicrosoftPowerpointWrapper.Tests
{
    public class LiedFormatteringTests
    {
        private ILiedFormatter _sut;

        [TestInitialize]
        public void Initialise()
        {
            _sut = new LiedFormatter();
        }

        [TestClass]
        public class LiturgieMethod : LiedFormatteringTests
        {
            [DataTestMethod]
            [DataRow("Psalm")]
            public void SpecifiekeLiturgieNaam_FormatteertStrak(string naam)
            {
                var regel = new SlideBuilder()
                    .SetDisplay(naamOverzicht: naam)
                    .Build();

                var geformatteerd = _sut.Liturgie(regel, true);

                Assert.AreEqual(naam, geformatteerd.Naam);
            }

            [DataTestMethod]
            [DataRow("Psalm", "100", 21, 22)]
            public void NaamEnSubnaamEnVersMetVolledigeContent_FormatteertStrak(string naam, string subNaam, int versNummer1, int versNummer2)
            {
                var regel = new SlideBuilder()
                    .SetDisplay(naam: naam, subnaam: subNaam)
                    .AddContent(versNummer1)
                    .AddContent(versNummer2)
                    .Build();

                var geformatteerd = _sut.Liturgie(regel, true);

                Assert.AreEqual(naam, geformatteerd.Naam);
                Assert.AreEqual(subNaam, geformatteerd.SubNaam);
                Assert.IsNull(geformatteerd.Verzen);
            }

            [DataTestMethod]
            [DataRow("10, 5")]
            public void ZonderDelen_ToontDefault(string defaultNaam)
            {
                var regel = new SlideBuilder()
                    .SetDisplayVersenGebruikDefault(defaultNaam)
                    .Build();

                var geformatteerd = _sut.Liturgie(regel, true);

                Assert.AreEqual(defaultNaam, geformatteerd.Verzen);
            }

            [DataTestMethod]
            [DataRow(new [] { 1, 2, 3, 4 })]
            public void VolledigeContent_ToonLeeg(int[] aansluitendeNummerLijst)
            {
                var regel = new SlideBuilder()
                    .SetDisplay(volledigeContent: true)
                    .AddContent(aansluitendeNummerLijst)
                    .Build();

                var geformatteerd = _sut.Liturgie(regel, true);

                Assert.IsNull(geformatteerd.Verzen);
            }

            [DataTestMethod]
            [DataRow(new [] { 1, 2, 3, 4 })]
            public void AansluitendeNummering_ToonGekoppeld(int[] aansluitendeNummerLijst)
            {
                var regel = new SlideBuilder()
                    .SetDisplay(volledigeContent: false)
                    .AddContent(aansluitendeNummerLijst)
                    .Build();

                var geformatteerd = _sut.Liturgie(regel, true);

                Assert.AreEqual($"{aansluitendeNummerLijst.First()} - {aansluitendeNummerLijst.Last()}", geformatteerd.Verzen);
            }

        }

        [TestClass]
        public class HuidigMethod : LiedFormatteringTests
        {

            [DataTestMethod]
            [DataRow("Psalm")]
            public void AlleenNaam_FormatteertStrak(string naam)
            {
                var regel = new SlideBuilder()
                    .SetDisplay(naam: naam)
                    .Build();

                var geformatteerd = _sut.Huidig(regel, null, true);

                Assert.AreEqual(naam, geformatteerd.Naam);
            }

            [DataTestMethod]
            [DataRow("Psalm", "100")]
            public void NaamEnSubnaam_FormatteertStrak(string naam, string subNaam)
            {
                var regel = new SlideBuilder()
                    .SetDisplay(naam: naam, subnaam: subNaam)
                    .Build();

                var geformatteerd = _sut.Huidig(regel, null, true);

                Assert.AreEqual(naam, geformatteerd.Naam);
                Assert.AreEqual(subNaam, geformatteerd.SubNaam);
            }

            [DataTestMethod]
            [DataRow("Psalm", "100", 21)]
            public void NaamEnSubnaamEnVersMetDeelVanContent_FormatteertStrak(string naam, string subNaam, int versNummer1)
            {
                var regel = new SlideBuilder()
                    .SetDisplay(naam: naam, subnaam: subNaam, volledigeContent: false)
                    .AddContent(versNummer1)
                    .Build();

                var geformatteerd = _sut.Huidig(regel, null, true);

                Assert.AreEqual(naam, geformatteerd.Naam);
                Assert.AreEqual(subNaam, geformatteerd.SubNaam);
                Assert.AreEqual($"{versNummer1}", geformatteerd.Verzen);
            }

            [DataTestMethod]
            [DataRow("Psalm", "100", 21, 22)]
            public void NaamEnSubnaamEnVersMetVolledigeContent_FormatteertStrak(string naam, string subNaam, int versNummer1, int versNummer2)
            {
                var regelBuilder = new SlideBuilder()
                    .SetDisplay(naam: naam, subnaam: subNaam)
                    .AddContent(versNummer1)
                    .AddContent(versNummer2);
                var regel = regelBuilder.Build();

                var geformatteerd = _sut.Huidig(regel, regelBuilder.LiturgieContent.First(), true);

                Assert.AreEqual(naam, geformatteerd.Naam);
                Assert.AreEqual(subNaam, geformatteerd.SubNaam);
                Assert.AreEqual($"{versNummer1}, {versNummer2}", geformatteerd.Verzen);
            }

            [DataTestMethod]
            [DataRow(new [] { 1, 2, 3, 4 })]
            public void VolledigeContent_ToonEersteLosToonRestGekoppeld(int[] aansluitendeNummerLijst)
            {
                var regelBuilder = new SlideBuilder()
                    .SetDisplay(volledigeContent: true)
                    .AddContent(aansluitendeNummerLijst);
                var regel = regelBuilder.Build();

                var geformatteerd = _sut.Huidig(regel, regelBuilder.LiturgieContent.First(), true);

                Assert.AreEqual($"{regelBuilder.LiturgieContent.First().Nummer}, {regelBuilder.LiturgieContent.Skip(1).First().Nummer} - {regelBuilder.LiturgieContent.Last().Nummer}", geformatteerd.Verzen);
            }

            [DataTestMethod]
            [DataRow(new [] { 1, 2, 3, 4 })]
            public void AansluitendeNummering_ToonEersteLosToonRestGekoppeld(int[] aansluitendeNummerLijst)
            {
                var regelBuilder = new SlideBuilder()
                    .SetDisplay(volledigeContent: false)
                    .AddContent(aansluitendeNummerLijst);
                var regel = regelBuilder.Build();

                var geformatteerd = _sut.Huidig(regel, regelBuilder.LiturgieContent.First(), true);

                Assert.AreEqual($"{regelBuilder.LiturgieContent.First().Nummer}, {regelBuilder.LiturgieContent.Skip(1).First().Nummer} - {regelBuilder.LiturgieContent.Last().Nummer}", geformatteerd.Verzen);
            }

            [DataTestMethod]
            [DataRow(new [] { 1, 2, 3, 4, 5, 7 })]
            public void AansluitendeNummeringMetLosNummer_ToonLosToonReeksToonLos(int[] aansluitendeNummerLijstMetLaasteLos)
            {
                var regelBuilder = new SlideBuilder()
                    .SetDisplay(volledigeContent: false)
                    .AddContent(aansluitendeNummerLijstMetLaasteLos);
                var regel = regelBuilder.Build();

                var geformatteerd = _sut.Huidig(regel, regelBuilder.LiturgieContent.First(), true);

                Assert.AreEqual($"{regelBuilder.LiturgieContent.First().Nummer}, {regelBuilder.LiturgieContent.Skip(1).First().Nummer} - {regelBuilder.LiturgieContent.OrderByDescending(n => n.Nummer).Skip(1).First().Nummer}, {regelBuilder.LiturgieContent.Last().Nummer}", geformatteerd.Verzen);
            }

            [DataTestMethod]
            [DataRow(new [] { 5, 6, 7, 12, 14 })]
            public void AansluitendeNummeringMetLosNummer_ToonLos(int[] aansluitendeNummerLijstMetLaaste2Los)
            {
                var regelBuilder = new SlideBuilder()
                    .SetDisplay(volledigeContent: false)
                    .AddContent(aansluitendeNummerLijstMetLaaste2Los);
                var regel = regelBuilder.Build();

                var geformatteerd = _sut.Huidig(regel, regelBuilder.LiturgieContent.First(), true);

                Assert.AreEqual(string.Join(", ", regelBuilder.LiturgieContent.Select(n => n.Nummer)), geformatteerd.Verzen);
            }
        }
    }
}
