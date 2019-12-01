// Copyright 2019 door Erik de Roos
using Generator.Database.Models;
using System.Collections.Generic;

namespace Generator.Database
{
    public interface ILiturgieDatabase
    {
        IOplossing KrijgItem(VerwerkingType alsType, string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen, LiturgieSettings settings);
    }
}
