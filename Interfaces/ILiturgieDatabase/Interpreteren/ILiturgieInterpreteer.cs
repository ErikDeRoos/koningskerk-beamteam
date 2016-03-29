// Copyright 2016 door Erik de Roos

namespace ILiturgieDatabase {
    public interface ILiturgieInterpreteer
    {
        ILiturgieInterpretatie VanTekstregel(string regels);
    }
}
