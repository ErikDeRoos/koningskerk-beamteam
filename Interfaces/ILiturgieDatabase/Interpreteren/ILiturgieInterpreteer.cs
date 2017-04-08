// Copyright 2017 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase {
    public interface ILiturgieInterpreteer
    {
        ILiturgieInterpretatie VanTekstregel(string regels);
        IEnumerable<ILiturgieInterpretatie> VanTekstregels(string[] regels);
    }
}
