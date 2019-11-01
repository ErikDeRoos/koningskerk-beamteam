// Copyright 2017 door Erik de Roos
using ILiturgieDatabase;
using System.Collections.Generic;

namespace mppt.LiedPresentator
{
    public interface ILiedFormatter
    {
        LiedFormatResult Huidig(ISlideOpbouw regel, ILiturgieContent vanafDeel);
        LiedFormatResult Volgende(IEnumerable<ISlideOpbouw> volgenden, int overslaan = 0);
        LiedFormatResult Liturgie(ISlideOpbouw regel);
    }
}
