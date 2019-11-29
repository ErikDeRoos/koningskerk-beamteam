// Copyright 2016 door Erik de Roos
using Generator.Database.Models;
using System.Collections.Generic;
using System.Threading;

namespace mppt.RegelVerwerking
{
    interface IVerwerk
    {
        IVerwerkResultaat Verwerk(ISlideInhoud regel, IEnumerable<ISlideOpbouw> volgenden, CancellationToken token);
    }
}
