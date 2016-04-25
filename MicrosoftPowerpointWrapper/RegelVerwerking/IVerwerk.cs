// Copyright 2016 door Erik de Roos
using ILiturgieDatabase;

namespace mppt.RegelVerwerking
{
    interface IVerwerk
    {
        IVerwerkResultaat Verwerk(ILiturgieRegel regel, ILiturgieRegel volgende);
    }
}
