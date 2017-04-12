// Copyright 2016 door Erik de Roos
using ILiturgieDatabase;
using System.Collections.Generic;

namespace mppt.RegelVerwerking
{
    interface IVerwerk
    {
        IVerwerkResultaat Verwerk(ILiturgieRegel regel, IEnumerable<ILiturgieRegel> volgenden);
    }
}
