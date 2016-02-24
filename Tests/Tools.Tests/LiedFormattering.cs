using NSubstitute;
using ILiturgieDatabase;
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;

namespace Tools.Tests
{
    public class LiedFormattering
    {
        [TestCase("Psalm")]
        public void LiedNaam_AlleenNaam_FormatteertStrak(string naam)
        {
            var liturgieRegel = Substitute.For<ILiturgieRegel>();
            var liturgieRegelDisplay = Substitute.For<ILiturgieDisplay>();
            liturgieRegel.Display.ReturnsForAnyArgs(liturgieRegelDisplay);
            liturgieRegelDisplay.Naam.ReturnsForAnyArgs(naam);
            liturgieRegelDisplay.SubNaam.ReturnsForAnyArgs((string)null);
            liturgieRegelDisplay.VersenGebruikDefault.ReturnsForAnyArgs((string)null);

            var geformatteerd = Tools.LiedFormattering.LiedNaam(liturgieRegel);

            Assert.That(geformatteerd, Is.EqualTo(naam));
        }

        [TestCase("Psalm", "100")]
        public void LiedNaam_NaamEnSubnaam_FormatteertStrak(string naam, string subNaam)
        {
            var liturgieRegel = Substitute.For<ILiturgieRegel>();
            var liturgieRegelDisplay = Substitute.For<ILiturgieDisplay>();
            liturgieRegel.Display.ReturnsForAnyArgs(liturgieRegelDisplay);
            liturgieRegelDisplay.Naam.ReturnsForAnyArgs(naam);
            liturgieRegelDisplay.SubNaam.ReturnsForAnyArgs(subNaam);
            liturgieRegelDisplay.VersenGebruikDefault.ReturnsForAnyArgs((string)null);

            var geformatteerd = Tools.LiedFormattering.LiedNaam(liturgieRegel);

            Assert.That(geformatteerd, Is.EqualTo($"{naam} {subNaam}"));
        }

        [TestCase("Psalm", "100", 21)]
        public void LiedNaam_NaamEnSubnaamEnVersMetDeelVanContent_FormatteertStrak(string naam, string subNaam, int versNummer1)
        {
            var liturgieRegel = Substitute.For<ILiturgieRegel>();
            var liturgieRegelDisplay = Substitute.For<ILiturgieDisplay>();
            var liturgieRegelContent1 = Substitute.For<ILiturgieContent>();
            liturgieRegel.Display.ReturnsForAnyArgs(liturgieRegelDisplay);
            liturgieRegelDisplay.Naam.ReturnsForAnyArgs(naam);
            liturgieRegelDisplay.SubNaam.ReturnsForAnyArgs(subNaam);
            liturgieRegelDisplay.VolledigeContent.ReturnsForAnyArgs(false);
            liturgieRegelDisplay.VersenGebruikDefault.ReturnsForAnyArgs((string)null);
            liturgieRegelContent1.Nummer.ReturnsForAnyArgs(versNummer1);
            liturgieRegel.Content.ReturnsForAnyArgs(new List<ILiturgieContent>() { liturgieRegelContent1 });

            var geformatteerd = Tools.LiedFormattering.LiedNaam(liturgieRegel);

            Assert.That(geformatteerd, Is.EqualTo($"{naam} {subNaam}: {versNummer1}"));
        }

        [TestCase("Psalm", "100", 21, 22)]
        public void LiedNaam_NaamEnSubnaamEnVersMetVolledigeContentInBeeld_FormatteertStrak(string naam, string subNaam, int versNummer1, int versNummer2)
        {
            var liturgieRegel = Substitute.For<ILiturgieRegel>();
            var liturgieRegelDisplay = Substitute.For<ILiturgieDisplay>();
            var liturgieRegelContent1 = Substitute.For<ILiturgieContent>();
            var liturgieRegelContent2 = Substitute.For<ILiturgieContent>();
            liturgieRegel.Display.ReturnsForAnyArgs(liturgieRegelDisplay);
            liturgieRegelDisplay.Naam.ReturnsForAnyArgs(naam);
            liturgieRegelDisplay.SubNaam.ReturnsForAnyArgs(subNaam);
            liturgieRegelDisplay.VolledigeContent.ReturnsForAnyArgs(true);
            liturgieRegelDisplay.VersenGebruikDefault.ReturnsForAnyArgs((string)null);
            liturgieRegelContent1.Nummer.ReturnsForAnyArgs(versNummer1);
            liturgieRegelContent2.Nummer.ReturnsForAnyArgs(versNummer2);
            liturgieRegel.Content.ReturnsForAnyArgs(new List<ILiturgieContent>() { liturgieRegelContent1, liturgieRegelContent2 });

            var geformatteerd = Tools.LiedFormattering.LiedNaam(liturgieRegel);

            Assert.That(geformatteerd, Is.EqualTo($"{naam} {subNaam}"));
        }

        [TestCase("Psalm", "100", 21, 22)]
        public void LiedNaam_NaamEnSubnaamEnVersMetVolledigeContentUitBeeld_FormatteertStrak(string naam, string subNaam, int versNummer1, int versNummer2)
        {
            var liturgieRegel = Substitute.For<ILiturgieRegel>();
            var liturgieRegelDisplay = Substitute.For<ILiturgieDisplay>();
            var liturgieRegelContent1 = Substitute.For<ILiturgieContent>();
            var liturgieRegelContent2 = Substitute.For<ILiturgieContent>();
            liturgieRegel.Display.ReturnsForAnyArgs(liturgieRegelDisplay);
            liturgieRegelDisplay.Naam.ReturnsForAnyArgs(naam);
            liturgieRegelDisplay.SubNaam.ReturnsForAnyArgs(subNaam);
            liturgieRegelDisplay.VolledigeContent.ReturnsForAnyArgs(true);
            liturgieRegelDisplay.VersenGebruikDefault.ReturnsForAnyArgs((string)null);
            liturgieRegelContent1.Nummer.ReturnsForAnyArgs(versNummer1);
            liturgieRegelContent2.Nummer.ReturnsForAnyArgs(versNummer2);
            liturgieRegel.Content.ReturnsForAnyArgs(new List<ILiturgieContent>() { liturgieRegelContent1, liturgieRegelContent2 });

            var geformatteerd = Tools.LiedFormattering.LiedNaam(liturgieRegel, liturgieRegelContent1);

            Assert.That(geformatteerd, Is.EqualTo($"{naam} {subNaam}: {versNummer1}, {versNummer2}"));
        }

        [TestCase("10, 5")]
        public void LiedVerzen_ZonderDelen_ToontDefault(string defaultNaam)
        {
            var liturgieRegelDisplay = Substitute.For<ILiturgieDisplay>();
            liturgieRegelDisplay.VersenGebruikDefault.ReturnsForAnyArgs(defaultNaam);

            var verwerkt = Tools.LiedFormattering.LiedVerzen(liturgieRegelDisplay, false, null);

            Assert.That(verwerkt, Is.EqualTo(defaultNaam));
        }
    }
}
