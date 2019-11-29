// Copyright 2017 door Erik de Roos
using Generator.Database.Models;
using System.Collections.Generic;

namespace mppt.LiedPresentator
{
    public interface ILiedFormatter
    {
        LiedFormatResult Huidig(ISlideInhoud regel, ILiturgieContent vanafDeel);
        LiedFormatResult Volgende(IEnumerable<ISlideOpbouw> volgenden, int overslaan = 0);
        LiedFormatResult Liturgie(ISlideInhoud regel);
    }
}
