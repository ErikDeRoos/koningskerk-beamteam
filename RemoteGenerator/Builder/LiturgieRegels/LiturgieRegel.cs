using ILiturgieDatabase;
using System.Linq;
using System.Collections.Generic;

namespace RemoteGenerator.Builder.LiturgieRegels
{
    class LiturgieRegel : ILiturgieRegel
    {
        public IEnumerable<ILiturgieContent> Content { get; set; }

        public ILiturgieDisplay Display { get; set; }

        public bool TonenInOverzicht { get; set; }

        public bool TonenInVolgende { get; set; }

        public bool VerwerkenAlsSlide { get; set; }

        public LiturgieRegel(ConnectTools.Berichten.LiturgieRegel vanRegel)
        {
            Content = vanRegel.Content?.Select(c => new LiturgieContent(c)).ToList();
            Display = new LiturgieDisplay(vanRegel.Display);
            TonenInOverzicht = vanRegel.TonenInOverzicht;
            TonenInVolgende = vanRegel.TonenInVolgende;
            VerwerkenAlsSlide = vanRegel.VerwerkenAlsSlide;
        }
    }
}
