using ILiturgieDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mppt.LiedPresentator
{
    public interface ILiedFormatter
    {
        LiedFormatResult Huidig(ILiturgieRegel regel, ILiturgieContent vanafDeel);
        LiedFormatResult Volgende(ILiturgieRegel volgende);
        LiedFormatResult Liturgie(ILiturgieRegel regel);
    }
}
