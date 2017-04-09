// Copyright 2017 door Erik de Roos
using ILiturgieDatabase;
using System.Collections.Generic;

namespace mppt.LiedPresentator
{
    public interface ILiedFormatter
    {
        LiedFormatResult Huidig(ILiturgieRegel regel, ILiturgieContent vanafDeel);
        LiedFormatResult Volgende(IEnumerable<ILiturgieRegel> volgenden, int overslaan = 0);
        LiedFormatResult Liturgie(ILiturgieRegel regel);
    }
}
