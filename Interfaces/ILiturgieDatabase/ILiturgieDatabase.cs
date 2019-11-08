// Copyright 2019 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase
{
    public interface ILiturgieDatabase
    {
        IOplossing KrijgItem(VerwerkingType alsType, string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen, LiturgieSettings settings);
    }
}
