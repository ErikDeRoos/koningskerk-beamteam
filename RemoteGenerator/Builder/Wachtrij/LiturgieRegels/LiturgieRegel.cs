// Copyright 2016 door Erik de Roos
using ILiturgieDatabase;
using System.Linq;
using System.Collections.Generic;
using System;

namespace RemoteGenerator.Builder.Wachtrij.LiturgieRegels
{
    class LiturgieRegel : ILiturgieRegel
    {
        public IEnumerable<ILiturgieContent> Content { get; set; }

        public ILiturgieDisplay Display { get; set; }

        public bool TonenInOverzicht { get; set; }

        public bool TonenInVolgende { get; set; }

        public bool VerwerkenAlsSlide { get; set; }

        public LiturgieRegel(ConnectTools.Berichten.LiturgieRegel vanRegel, Func<ConnectTools.Berichten.StreamToken, BestandStreamToken> bestandStreamTokenFactory)
        {
            Content = vanRegel.Content?.Select(c => new LiturgieContent(c, bestandStreamTokenFactory)).ToList();
            Display = new LiturgieDisplay(vanRegel.Display);
            TonenInOverzicht = vanRegel.TonenInOverzicht;
            TonenInVolgende = vanRegel.TonenInVolgende;
            VerwerkenAlsSlide = vanRegel.VerwerkenAlsSlide;
        }
    }
}
