// Copyright 2017 door Erik de Roos
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILiturgieDatabase
{
    public interface IVrijZoekresultaat
    {
        string ZoekTerm { get; }
        IEnumerable<string> Mogelijkheden { get; }
    }
}
