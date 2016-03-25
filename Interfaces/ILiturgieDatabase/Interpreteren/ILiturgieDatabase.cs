﻿using System.Collections.Generic;

namespace ILiturgieDatabase
{
    public interface ILiturgieDatabase
    {
        IZoekresultaat ZoekOnderdeel(string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen = null);
        IZoekresultaat ZoekOnderdeel(VerwerkingType alsType, string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen = null);
    }
}
