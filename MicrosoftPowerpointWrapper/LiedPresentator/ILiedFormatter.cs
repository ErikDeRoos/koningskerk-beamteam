// Copyright 2019 door Erik de Roos
using Generator.Database.Models;
using System.Collections.Generic;

namespace mppt.LiedPresentator
{
    public interface ILiedFormatter
    {
        LiedFormatResult Huidig(ISlideInhoud regel, ILiturgieContent vanafDeel, bool verkortBijVolledigeContent);
        LiedFormatResult Volgende(IEnumerable<ISlideOpbouw> volgenden, int overslaan, bool verkortBijVolledigeContent);
        LiedFormatResult Liturgie(ISlideInhoud regel, bool verkortBijVolledigeContent);
    }
}
