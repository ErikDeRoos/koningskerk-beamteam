// Copyright 2016 door Erik de Roos
using ILiturgieDatabase;
using System.Collections.Generic;
using System.Threading;

namespace mppt.RegelVerwerking
{
    interface IVerwerk
    {
        IVerwerkResultaat Verwerk(ILiturgieRegel regel, IEnumerable<ILiturgieRegel> volgenden, CancellationToken token);
    }
}
