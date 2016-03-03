using ILiturgieDatabase;
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using FakeItEasy;

namespace Tools.Tests
{
    public class LiedFormattering
    {
        [TestCase("Psalm")]
        public void LiedNaam_AlleenNaam_FormatteertStrak(string naam)
        {
            var liturgieRegel = A.Fake<ILiturgieRegel>();
            A.CallTo(() => liturgieRegel.Display.Naam).Returns(naam);
            A.CallTo(() => liturgieRegel.Display.SubNaam).Returns(null);

            var geformatteerd = Tools.LiedFormattering.LiedNaam(liturgieRegel);

            Assert.That(geformatteerd, Is.EqualTo(naam));
        }

        [TestCase("Psalm", "100")]
        public void LiedNaam_NaamEnSubnaam_FormatteertStrak(string naam, string subNaam)
        {
            var liturgieRegel = A.Fake<ILiturgieRegel>();
            A.CallTo(() => liturgieRegel.Display.Naam).Returns(naam);
            A.CallTo(() => liturgieRegel.Display.SubNaam).Returns(subNaam);

            var geformatteerd = Tools.LiedFormattering.LiedNaam(liturgieRegel);

            Assert.That(geformatteerd, Is.EqualTo($"{naam} {subNaam}"));
        }

        [TestCase("Psalm", "100", 21)]
        public void LiedNaam_NaamEnSubnaamEnVersMetDeelVanContent_FormatteertStrak(string naam, string subNaam, int versNummer1)
        {
            var liturgieRegel = A.Fake<ILiturgieRegel>();
            A.CallTo(() => liturgieRegel.Display.Naam).Returns(naam);
            A.CallTo(() => liturgieRegel.Display.SubNaam).Returns(subNaam);
            A.CallTo(() => liturgieRegel.Display.VolledigeContent).Returns(false);
            var liturgieRegelContent1 = A.Fake<ILiturgieContent>();
            A.CallTo(() => liturgieRegelContent1.Nummer).Returns(versNummer1);
            A.CallTo(() => liturgieRegel.Content).Returns(new List<ILiturgieContent>() { liturgieRegelContent1 });

            var geformatteerd = Tools.LiedFormattering.LiedNaam(liturgieRegel);

            Assert.That(geformatteerd, Is.EqualTo($"{naam} {subNaam}: {versNummer1}"));
        }

        [TestCase("Psalm", "100", 21, 22)]
        public void LiedNaam_NaamEnSubnaamEnVersMetVolledigeContentInBeeld_FormatteertStrak(string naam, string subNaam, int versNummer1, int versNummer2)
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

            var geformatteerd = Tools.LiedFormattering.LiedNaam(liturgieRegel);

            Assert.That(geformatteerd, Is.EqualTo($"{naam} {subNaam}"));
        }

        [TestCase("Psalm", "100", 21, 22)]
        public void LiedNaam_NaamEnSubnaamEnVersMetVolledigeContentUitBeeld_FormatteertStrak(string naam, string subNaam, int versNummer1, int versNummer2)
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

            var geformatteerd = Tools.LiedFormattering.LiedNaam(liturgieRegel, liturgieRegelContent1);

            Assert.That(geformatteerd, Is.EqualTo($"{naam} {subNaam}: {versNummer1}, {versNummer2}"));
        }

        [TestCase("10, 5")]
        public void LiedVerzen_ZonderDelen_ToontDefault(string defaultNaam)
        {
            var liturgieRegelDisplay = A.Fake<ILiturgieDisplay>();
            A.CallTo(() => liturgieRegelDisplay.VersenGebruikDefault.Tekst).Returns(defaultNaam);

            var verwerkt = Tools.LiedFormattering.LiedVerzen(liturgieRegelDisplay, false, null);

            Assert.That(verwerkt, Is.EqualTo(defaultNaam));
        }

        [TestCase(new int[] { 1, 2, 3, 4 }, true, true, "10, 5")]
        [TestCase(new int[] { 1, 2, 3, 4 }, true, false, "10, 5")]
        [TestCase(new int[] { 1, 2, 3, 4 }, false, true, "10, 5")]
        [TestCase(new int[] { 1, 2, 3, 4 }, false, false, "10, 5")]
        [TestCase(new int[] { 1, 2, 3, 4 }, false, false, "")]
        public void LiedVerzen_MetDefaultNotNull_ToontAltijdDefault(int[] aansluitendeNummerLijst, bool inBeeld, bool volledigeContent, string defaultNaam)
        {
            var liturgieRegelDisplay = A.Fake<ILiturgieDisplay>();
            A.CallTo(() => liturgieRegelDisplay.VersenGebruikDefault.Tekst).Returns(defaultNaam);
            A.CallTo(() => liturgieRegelDisplay.VersenGebruikDefault.Gebruik).Returns(true);
            var nummerLijst = PrepareContentLijst(aansluitendeNummerLijst).ToList();

            var verwerkt = Tools.LiedFormattering.LiedVerzen(liturgieRegelDisplay, false, nummerLijst);

            Assert.That(verwerkt, Is.EqualTo(defaultNaam));
        }

        [TestCase(new int[] { 1, 2, 3, 4 })]
        public void LiedVerzen_VolledigeContentNietInBeeld_ToonLeeg(int[] aansluitendeNummerLijst)
        {
            var liturgieRegelDisplay = A.Fake<ILiturgieDisplay>();
            A.CallTo(() => liturgieRegelDisplay.VolledigeContent).Returns(true);
            var nummerLijst = PrepareContentLijst(aansluitendeNummerLijst).ToList();

            var verwerkt = Tools.LiedFormattering.LiedVerzen(liturgieRegelDisplay, false, nummerLijst);

            Assert.That(verwerkt, Is.EqualTo(string.Empty));
        }

        [TestCase(new int[] { 1, 2, 3, 4 })]
        public void LiedVerzen_VolledigeContentInBeeld_ToonEersteLosToonRestGekoppeld(int[] aansluitendeNummerLijst)
        {
            var liturgieRegelDisplay = A.Fake<ILiturgieDisplay>();
            A.CallTo(() => liturgieRegelDisplay.VolledigeContent).Returns(true);
            var nummerLijst = PrepareContentLijst(aansluitendeNummerLijst).ToList();

            var verwerkt = Tools.LiedFormattering.LiedVerzen(liturgieRegelDisplay, true, nummerLijst);

            Assert.That(verwerkt, Is.EqualTo($"{nummerLijst.First().Nummer}, {nummerLijst.Skip(1).First().Nummer} - {nummerLijst.Last().Nummer}"));
        }

        [TestCase(new int[] { 1, 2, 3, 4 })]
        public void LiedVerzen_AansluitendeNummeringNietInBeeld_ToonGekoppeld(int[] aansluitendeNummerLijst)
        {
            var liturgieRegelDisplay = A.Fake<ILiturgieDisplay>();
            A.CallTo(() => liturgieRegelDisplay.VolledigeContent).Returns(false);
            var nummerLijst = PrepareContentLijst(aansluitendeNummerLijst).ToList();

            var verwerkt = Tools.LiedFormattering.LiedVerzen(liturgieRegelDisplay, false, nummerLijst);

            Assert.That(verwerkt, Is.EqualTo($"{nummerLijst.First().Nummer} - {nummerLijst.Last().Nummer}"));
        }

        [TestCase(new int[] { 1, 2, 3, 4 })]
        public void LiedVerzen_AansluitendeNummeringInBeeld_ToonEersteLosToonRestGekoppeld(int[] aansluitendeNummerLijst)
        {
            var liturgieRegelDisplay = A.Fake<ILiturgieDisplay>();
            A.CallTo(() => liturgieRegelDisplay.VolledigeContent).Returns(false);
            var nummerLijst = PrepareContentLijst(aansluitendeNummerLijst).ToList();

            var verwerkt = Tools.LiedFormattering.LiedVerzen(liturgieRegelDisplay, true, nummerLijst);

            Assert.That(verwerkt, Is.EqualTo($"{nummerLijst.First().Nummer}, {nummerLijst.Skip(1).First().Nummer} - {nummerLijst.Last().Nummer}"));
        }

        [TestCase(new int[] { 1, 2, 3, 4, 6 })]
        public void LiedVerzen_AansluitendeNummeringMetLosNummer_ToonLosToonReeksToonLos(int[] aansluitendeNummerLijstMetLaasteLos)
        {
            var liturgieRegelDisplay = A.Fake<ILiturgieDisplay>();
            A.CallTo(() => liturgieRegelDisplay.VolledigeContent).Returns(false);
            var nummerLijst = PrepareContentLijst(aansluitendeNummerLijstMetLaasteLos).ToList();

            var verwerkt = Tools.LiedFormattering.LiedVerzen(liturgieRegelDisplay, true, nummerLijst);

            Assert.That(verwerkt, Is.EqualTo($"{nummerLijst.First().Nummer}, {nummerLijst.Skip(1).First().Nummer} - {nummerLijst.OrderByDescending(n => n.Nummer).Skip(1).First().Nummer}, {nummerLijst.Last().Nummer}"));
        }

        [TestCase(new int[] { 5, 6, 7, 12, 14 })]
        public void LiedVerzen_AansluitendeNummeringMetLosNummerInBeeld_ToonLos(int[] aansluitendeNummerLijstMetLaaste2Los)
        {
            var liturgieRegelDisplay = A.Fake<ILiturgieDisplay>();
            A.CallTo(() => liturgieRegelDisplay.VolledigeContent).Returns(false);
            var nummerLijst = PrepareContentLijst(aansluitendeNummerLijstMetLaaste2Los).ToList();

            var verwerkt = Tools.LiedFormattering.LiedVerzen(liturgieRegelDisplay, true, nummerLijst);

            Assert.That(verwerkt, Is.EqualTo(string.Join(", ", nummerLijst.Select(n => n.Nummer))));
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
