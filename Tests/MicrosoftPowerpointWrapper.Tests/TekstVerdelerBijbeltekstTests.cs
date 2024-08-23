using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftPowerpointWrapper.Tests.Builders;
using mppt.RegelVerwerking;
using System.Linq;

namespace MicrosoftPowerpointWrapper.Tests
{
    [TestClass]
    public class TekstVerdelerBijbeltekstTests
    {
        [TestMethod]
        public void OpdelenPerSlide_VolleRegelsZonderSlideonderbrekingToegestaan_VerdeeltOverSlides()
        {
            var regel = new BijbeltekstBuilder()
                .VoegVerzenToe(3, BijbeltekstBuilder.MaxRowLength - 5);
            var regelsPerSlide = 2;

            var geformatteerd = TekstVerdelerBijbeltekst.OpdelenPerSlide(regel.Tekst, regelsPerSlide, regel.LengteBerekenaar, true);

            Assert.AreEqual(2, geformatteerd.Count());
        }

        [TestMethod]
        public void OpdelenPerSlide_HalveRegelsZonderSlideonderbrekingToegestaan_VerdeeltOverSlides()
        {
            var regel = new BijbeltekstBuilder()
                .VoegVerzenToe(5, BijbeltekstBuilder.MaxRowLength / 2 - 1);
            var regelsPerSlide = 2;

            var geformatteerd = TekstVerdelerBijbeltekst.OpdelenPerSlide(regel.Tekst, regelsPerSlide, regel.LengteBerekenaar, true);

            Assert.AreEqual(2, geformatteerd.Count());
        }

        // testen of zonder onderbreking automatisch nieuwe slide maakt -> is van 2 regels op een 3 regel slide naar de volgende gaat.
        [TestMethod]
        public void OpdelenPerSlide_RegelPastNetOpSlideZonderSlideonderbrekingToegestaan_VerdeeltOverSlides()
        {
            var regel = new BijbeltekstBuilder()
                .VoegVerzenToe(3, BijbeltekstBuilder.MaxRowLength * 2 - 6);
            var regelsPerSlide = 3;

            var geformatteerd = TekstVerdelerBijbeltekst.OpdelenPerSlide(regel.Tekst, regelsPerSlide, regel.LengteBerekenaar, true);

            Assert.AreEqual(3, geformatteerd.Count());
        }

        // testen of zonder onderbreking wel onderbreekt bij te lange regel
        [TestMethod]
        public void OpdelenPerSlide_RegelPastNietOpSlideZonderSlideonderbrekingToegestaan_KniptWelOp()
        {
            var regel = new BijbeltekstBuilder()
                .VoegVerzenToe(1, BijbeltekstBuilder.MaxRowLength * 3);
            var regelsPerSlide = 2;

            var geformatteerd = TekstVerdelerBijbeltekst.OpdelenPerSlide(regel.Tekst, regelsPerSlide, regel.LengteBerekenaar, true);

            Assert.AreEqual(2, geformatteerd.Count());
        }

        // testen of met onderbreking mooi uitvult
        [TestMethod]
        public void OpdelenPerSlide_RegelPastNietOpSlideMetSlideonderbrekingToegestaan_KniptNetjesOp()
        {
            var regel = new BijbeltekstBuilder()
                .VoegVerzenToe(3, (BijbeltekstBuilder.MaxRowLength - 5) * 3);
            var regelsPerSlide = 2;

            var geformatteerd = TekstVerdelerBijbeltekst.OpdelenPerSlide(regel.Tekst, regelsPerSlide, regel.LengteBerekenaar, false);

            Assert.AreEqual(6, geformatteerd.Count());
        }
    }
}
