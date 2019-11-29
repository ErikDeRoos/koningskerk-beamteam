using Generator.Database.Models;
using Moq;
using System.Collections.Generic;

namespace MicrosoftPowerpointWrapper.Tests.Builders
{
    public class SlideBuilder
    {
        private readonly Mock<ISlideOpbouw> _slideOpbouw = new Mock<ISlideOpbouw>();
        private readonly Mock<ILiturgieDisplay> _liturgieDisplay = new Mock<ILiturgieDisplay>();

        private List<ILiturgieContent> _liturgieContent = null;
        public IEnumerable<ILiturgieContent> LiturgieContent { get { return _liturgieContent; } }

        public ISlideOpbouw Build()
        {
            _slideOpbouw.SetupGet(x => x.Display).Returns(_liturgieDisplay.Object);
            _slideOpbouw.SetupGet(x => x.Content).Returns(_liturgieContent);
            return _slideOpbouw.Object;
        }

        public SlideBuilder SetDisplay(string naam = null, string naamOverzicht = null, string subnaam = null, bool volledigeContent = true)
        {
            _liturgieDisplay.SetupGet(x => x.Naam).Returns(naam);
            _liturgieDisplay.SetupGet(x => x.SubNaam).Returns(subnaam);
            _liturgieDisplay.SetupGet(x => x.NaamOverzicht).Returns(naamOverzicht);
            _liturgieDisplay.SetupGet(x => x.VolledigeContent).Returns(volledigeContent);
            return this;
        }

        public SlideBuilder SetDisplayVersenGebruikDefault(string tekst)
        {
            _liturgieDisplay.SetupGet(x => x.VersenGebruikDefault).Returns(tekst);
            return this;
        }

        public SlideBuilder AddContent(int nr)
        {
            if (_liturgieContent == null)
                _liturgieContent = new List<ILiturgieContent>();

            var content = new Mock<ILiturgieContent>();
            content.SetupGet(x => x.Nummer).Returns(nr);
            _liturgieContent.Add(content.Object);
            return this;
        }

        public SlideBuilder AddContent(int[] nrs)
        {
            foreach (var nr in nrs)
                AddContent(nr);
            return this;
        }
    }
}
