// Copyright 2016 door Erik de Roos
using Generator.Database.Models;
using Generator.LiturgieInterpretator.Models;
using mppt.Connect;
using mppt.LiedPresentator;
using System.Collections.Generic;

namespace mppt.RegelVerwerking
{
    interface IVerwerkFactory
    {
        IVerwerk Init(IMppApplication metApplicatie, IMppPresentatie toevoegenAanPresentatie, IMppFactory metFactory, ILiedFormatter gebruikLiedFormatter, IBuilderBuildSettings buildSettings,
                IBuilderBuildDefaults buildDefaults, IBuilderDependendFiles dependentFileList, IEnumerable<ISlideOpbouw> volledigeLiturgieOpVolgorde, ILengteBerekenaar lengteBerekenaar);
    }
}
