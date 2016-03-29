// Copyright 2016 door Erik de Roos
using ILiturgieDatabase;
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using FakeItEasy;
using mppt.LiedPresentator;

namespace MicrosoftPowerpointWrapper.Tests
{
    public class LiedFormattering
    {
        [TestCase("Psalm")]
        public void Liturgie_SpecifiekeLiturgieNaam_FormatteertStrak(string naam)
        {
            var liturgieRegel = A.Fake<ILiturgieRegel>();
            A.CallTo(() => liturgieRegel.Display.NaamOverzicht).Returns(naam);
            var sut = (new LiedFormatter()) as ILiedFormatter;

            var geformatteerd = sut.Liturgie(liturgieRegel);

            Assert.That(geformatteerd.Naam, Is.EqualTo(naam));
        }

        [TestCase("Psalm", "100", 21, 22)]
        public void Liturgie_NaamEnSubnaamEnVersMetVolledigeContent_FormatteertStrak(string naam, string subNaam, int versNummer1, int versNummer2)
        {
            var liturgieRegel = A.Fake<ILiturgieRegel>();
            A.CallTo(() => liturgieRegel.Display.NaamOverzicht).Returns(naam);
            A.CallTo(() => liturgieRegel.Display.SubNaam).Returns(subNaam);
            A.CallTo(() => liturgieRegel.Display.VolledigeContent).Returns(true);
            var liturgieRegelContent1 = A.Fake<ILiturgieContent>();
            A.CallTo(() => liturgieRegelContent1.Nummer).Returns(versNummer1);
            var liturgieRegelContent2 = A.Fake<ILiturgieContent>();
            A.CallTo(() => liturgieRegelContent2.Nummer).Returns(versNummer2);
            A.CallTo(() => liturgieRegel.Content).Returns(new List<ILiturgieContent>() { liturgieRegelContent1, liturgieRegelContent2 });
            var sut = (new LiedFormatter()) as ILiedFormatter;

            var geformatteerd = sut.Liturgie(liturgieRegel);

            Assert.That(geformatteerd.Naam, Is.EqualTo(naam));
            Assert.That(geformatteerd.SubNaam, Is.EqualTo(subNaam));
            Assert.That(geformatteerd.Verzen, Is.EqualTo(null));
        }

        [TestCase("10, 5")]
        public void Liturgie_ZonderDelen_ToontDefault(string defaultNaam)
        {
            var liturgieRegel = A.Fake<ILiturgieRegel>();
            A.CallTo(() => liturgieRegel.Content).Returns(null);
            A.CallTo(() => liturgieRegel.Display.VersenGebruikDefault.Tekst).Returns(defaultNaam);
            var sut = (new LiedFormatter()) as ILiedFormatter;

            var geformatteerd = sut.Liturgie(liturgieRegel);

            Assert.That(geformatteerd.Verzen, Is.EqualTo(defaultNaam));
        }

        [TestCase(new int[] { 1, 2, 3, 4 })]
        public void Liturgie_VolledigeContent_ToonLeeg(int[] aansluitendeNummerLijst)
        {
            var liturgieRegel = A.Fake<ILiturgieRegel>();
            A.CallTo(() => liturgieRegel.Display.VolledigeContent).Returns(true);
            A.CallTo(() => liturgieRegel.Content).Returns(PrepareContentLijst(aansluitendeNummerLijst).ToList());
            var sut = (new LiedFormatter()) as ILiedFormatter;

            var geformatteerd = sut.Liturgie(liturgieRegel);

            Assert.That(geformatteerd.Verzen, Is.EqualTo(null));
        }

        [TestCase("Psalm")]
        public void Huidig_AlleenNaam_FormatteertStrak(string naam)
        {
            var liturgieRegel = A.Fake<ILiturgieRegel>();
            A.CallTo(() => liturgieRegel.Display.Naam).Returns(naam);
            var sut = (new LiedFormatter()) as ILiedFormatter;

            var geformatteerd = sut.Huidig(liturgieRegel, null);

            Assert.That(geformatteerd.Naam, Is.EqualTo(naam));
        }

        [TestCase("Psalm", "100")]
        public void Huidig_NaamEnSubnaam_FormatteertStrak(string naam, string subNaam)
        {
            var liturgieRegel = A.Fake<ILiturgieRegel>();
            A.CallTo(() => liturgieRegel.Display.Naam).Returns(naam);
            A.CallTo(() => liturgieRegel.Display.SubNaam).Returns(subNaam);
            var sut = (new LiedFormatter()) as ILiedFormatter;

            var geformatteerd = sut.Huidig(liturgieRegel, null);

            Assert.That(geformatteerd.Naam, Is.EqualTo(naam));
            Assert.That(geformatteerd.SubNaam, Is.EqualTo(subNaam));
        }

        [TestCase("Psalm", "100", 21)]
        public void Huidig_NaamEnSubnaamEnVersMetDeelVanContent_FormatteertStrak(string naam, string subNaam, int versNummer1)
        {
            var liturgieRegel = A.Fake<ILiturgieRegel>();
            A.CallTo(() => liturgieRegel.Display.Naam).Returns(naam);
            A.CallTo(() => liturgieRegel.Display.SubNaam).Returns(subNaam);
            A.CallTo(() => liturgieRegel.Display.VolledigeContent).Returns(false);
            var liturgieRegelContent1 = A.Fake<ILiturgieContent>();
            A.CallTo(() => liturgieRegelContent1.Nummer).Returns(versNummer1);
            A.CallTo(() => liturgieRegel.Content).Returns(new List<ILiturgieContent>() { liturgieRegelContent1 });
            var sut = (new LiedFormatter()) as ILiedFormatter;

            var geformatteerd = sut.Huidig(liturgieRegel, null);

            Assert.That(geformatteerd.Naam, Is.EqualTo(naam));
            Assert.That(geformatteerd.SubNaam, Is.EqualTo(subNaam));
            Assert.That(geformatteerd.Verzen, Is.EqualTo($"{versNummer1}"));
        }

        [TestCase("Psalm", "100", 21, 22)]
        public void Huidig_NaamEnSubnaamEnVersMetVolledigeContent_FormatteertStrak(string naam, string subNaam, int versNummer1, int versNummer2)
        {
            var liturgieRegel = A.Fake<ILiturgieRegel>();
            A.CallTo(() => liturgieRegel.Display.Naam).Returns(naam);
            A.CallTo(() => liturgieRegel.Display.SubNaam).Returns(subNaam);
            A.CallTo(() => liturgieRegel.Display.VolledigeContent).Returns(true);
            var liturgieRegelContent1 = A.Fake<ILiturgieContent>();
            A.CallTo(() => liturgieRegelContent1.Nummer).Returns(versNummer1);
            var liturgieRegelContent2 = A.Fake<ILiturgieContent>();
            A.CallTo(() => liturgieRegelContent2.Nummer).Returns(versNummer2);
            A.CallTo(() => liturgieRegel.Content).Returns(new List<ILiturgieContent>() { liturgieRegelContent1, liturgieRegelContent2 });
            var sut = (new LiedFormatter()) as ILiedFormatter;

            var geformatteerd = sut.Huidig(liturgieRegel, liturgieRegelContent1);

            Assert.That(geformatteerd.Naam, Is.EqualTo(naam));
            Assert.That(geformatteerd.SubNaam, Is.EqualTo(subNaam));
            Assert.That(geformatteerd.Verzen, Is.EqualTo($"{versNummer1}, {versNummer2}"));
        }

        [TestCase(new int[] { 1, 2, 3, 4 })]
        public void Huidig_VolledigeContent_ToonEersteLosToonRestGekoppeld(int[] aansluitendeNummerLijst)
        {
            var liturgieRegel = A.Fake<ILiturgieRegel>();
            A.CallTo(() => liturgieRegel.Display.VolledigeContent).Returns(true);
            var nummerLijst = PrepareContentLijst(aansluitendeNummerLijst).ToList();
            A.CallTo(() => liturgieRegel.Content).Returns(nummerLijst);
            var sut = (new LiedFormatter()) as ILiedFormatter;

            var geformatteerd = sut.Huidig(liturgieRegel, nummerLijst.First());

            Assert.That(geformatteerd.Verzen, Is.EqualTo($"{nummerLijst.First().Nummer}, {nummerLijst.Skip(1).First().Nummer} - {nummerLijst.Last().Nummer}"));
        }

        [TestCase(new int[] { 1, 2, 3, 4 })]
        public void Liturgie_AansluitendeNummering_ToonGekoppeld(int[] aansluitendeNummerLijst)
        {
            var liturgieRegel = A.Fake<ILiturgieRegel>();
            A.CallTo(() => liturgieRegel.Display.VolledigeContent).Returns(false);
            var nummerLijst = PrepareContentLijst(aansluitendeNummerLijst).ToList();
            A.CallTo(() => liturgieRegel.Content).Returns(nummerLijst);
            var sut = (new LiedFormatter()) as ILiedFormatter;

            var geformatteerd = sut.Liturgie(liturgieRegel);

            Assert.That(geformatteerd.Verzen, Is.EqualTo($"{nummerLijst.First().Nummer} - {nummerLijst.Last().Nummer}"));
        }

        [TestCase(new int[] { 1, 2, 3, 4 })]
        public void Huidig_AansluitendeNummering_ToonEersteLosToonRestGekoppeld(int[] aansluitendeNummerLijst)
        {
            var liturgieRegel = A.Fake<ILiturgieRegel>();
            A.CallTo(() => liturgieRegel.Display.VolledigeContent).Returns(false);
            var nummerLijst = PrepareContentLijst(aansluitendeNummerLijst).ToList();
            A.CallTo(() => liturgieRegel.Content).Returns(nummerLijst);
            var sut = (new LiedFormatter()) as ILiedFormatter;

            var geformatteerd = sut.Huidig(liturgieRegel, nummerLijst.First());

            Assert.That(geformatteerd.Verzen, Is.EqualTo($"{nummerLijst.First().Nummer}, {nummerLijst.Skip(1).First().Nummer} - {nummerLijst.Last().Nummer}"));
        }

        [TestCase(new int[] { 1, 2, 3, 4, 6 })]
        public void Huidig_AansluitendeNummeringMetLosNummer_ToonLosToonReeksToonLos(int[] aansluitendeNummerLijstMetLaasteLos)
        {
            var liturgieRegel = A.Fake<ILiturgieRegel>();
            A.CallTo(() => liturgieRegel.Display.VolledigeContent).Returns(false);
            var nummerLijst = PrepareContentLijst(aansluitendeNummerLijstMetLaasteLos).ToList();
            A.CallTo(() => liturgieRegel.Content).Returns(nummerLijst);
            var sut = (new LiedFormatter()) as ILiedFormatter;

            var geformatteerd = sut.Huidig(liturgieRegel, nummerLijst.First());

            Assert.That(geformatteerd.Verzen, Is.EqualTo($"{nummerLijst.First().Nummer}, {nummerLijst.Skip(1).First().Nummer} - {nummerLijst.OrderByDescending(n => n.Nummer).Skip(1).First().Nummer}, {nummerLijst.Last().Nummer}"));
        }

        [TestCase(new int[] { 5, 6, 7, 12, 14 })]
        public void Huidig_AansluitendeNummeringMetLosNummer_ToonLos(int[] aansluitendeNummerLijstMetLaaste2Los)
        {
            var liturgieRegel = A.Fake<ILiturgieRegel>();
            A.CallTo(() => liturgieRegel.Display.VolledigeContent).Returns(false);
            var nummerLijst = PrepareContentLijst(aansluitendeNummerLijstMetLaaste2Los).ToList();
            A.CallTo(() => liturgieRegel.Content).Returns(nummerLijst);
            var sut = (new LiedFormatter()) as ILiedFormatter;

            var geformatteerd = sut.Huidig(liturgieRegel, nummerLijst.First());

            Assert.That(geformatteerd.Verzen, Is.EqualTo(string.Join(", ", nummerLijst.Select(n => n.Nummer))));
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
