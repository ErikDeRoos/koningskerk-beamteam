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

            var geformatteerd = Tools.LiedFormattering.LiedNaam(liturgieRegel);

            Assert.That(geformatteerd, Is.EqualTo($"{naam} {subNaam}"));
        }

        [TestCase("Psalm", "100", "21", 21)]
        public void LiedNaam_NaamEnSubnaamEnVers_FormatteertStrak(string naam, string subNaam, string defaultNaam, int versNummer1)
        {
            var liturgieRegel = Substitute.For<ILiturgieRegel>();
            var liturgieRegelDisplay = Substitute.For<ILiturgieDisplay>();
            var liturgieRegelContent = Substitute.For<ILiturgieContent>();
            liturgieRegel.Display.ReturnsForAnyArgs(liturgieRegelDisplay);
            liturgieRegelDisplay.Naam.ReturnsForAnyArgs(naam);
            liturgieRegelDisplay.SubNaam.ReturnsForAnyArgs(subNaam);
            liturgieRegelDisplay.VersenDefault.ReturnsForAnyArgs(defaultNaam);
            liturgieRegelContent.Nummer.ReturnsForAnyArgs(versNummer1);
            liturgieRegel.Content.ReturnsForAnyArgs(new List<ILiturgieContent>() { liturgieRegelContent });

            var geformatteerd = Tools.LiedFormattering.LiedNaam(liturgieRegel);

            Assert.That(geformatteerd, Is.EqualTo($"{naam} {subNaam}: {versNummer1}"));
        }

        [TestCase("10, 5")]
        public void LiedVerzen_NietAfleiden_ToontDefault(string defaultNaam)
        {
            var liturgieRegelDisplay = Substitute.For<ILiturgieDisplay>();
            liturgieRegelDisplay.VersenDefault.ReturnsForAnyArgs(defaultNaam);
            liturgieRegelDisplay.VersenAfleiden.ReturnsForAnyArgs(false);

            var verwerkt = Tools.LiedFormattering.LiedVerzen(liturgieRegelDisplay, false, null);

            Assert.That(verwerkt, Is.EqualTo(defaultNaam));
        }

        [TestCase("10, 5")]
        public void LiedVerzen_ZonderDelen_ToontDefault(string defaultNaam)
        {
            var liturgieRegelDisplay = Substitute.For<ILiturgieDisplay>();
            liturgieRegelDisplay.VersenDefault.ReturnsForAnyArgs(defaultNaam);

            var verwerkt = Tools.LiedFormattering.LiedVerzen(liturgieRegelDisplay, false, null);

            Assert.That(verwerkt, Is.EqualTo(defaultNaam));
        }
    }
}
