// Copyright 2016 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase {

    public interface ILiturgieLosOp
    {
        ILiturgieOplossing LosOp(ILiturgieInterpretatie item);
        ILiturgieOplossing LosOp(ILiturgieInterpretatie item, IEnumerable<ILiturgieMapmaskArg> masks);
        IEnumerable<ILiturgieOplossing> LosOp(IEnumerable<ILiturgieInterpretatie> items, IEnumerable<ILiturgieMapmaskArg> masks);
    }
}
