// Copyright 2016 door Erik de Roos
using ILiturgieDatabase;
using ISlideBuilder;
using mppt.Connect;
using mppt.LiedPresentator;
using System.Collections.Generic;

namespace mppt.RegelVerwerking
{
    interface IVerwerkFactory
    {
        IVerwerk Init(IMppApplication metApplicatie, IMppPresentatie toevoegenAanPresentatie, IMppFactory metFactory, ILiedFormatter gebruikLiedFormatter, IBuilderBuildSettings buildSettings,
                IBuilderBuildDefaults buildDefaults, IBuilderDependendFiles dependentFileList, IEnumerable<ILiturgieRegel> volledigeLiturgieOpVolgorde);
    }
}
