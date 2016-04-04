// Copyright 2016 door Erik de Roos
using System.Collections.Generic;

namespace ISettings
{
    public interface IInstellingen : IInstellingenBase
    {
        string DatabasePad { get; }
        string Templateliederen { get; }
        string Templatetheme { get; }
        string BijbelPad { get; }

        IEnumerable<IMapmask> Masks { get; }
        bool AddMask(IMapmask mask);
        void ClearMasks();

        string FullDatabasePath { get; }
        string FullBijbelPath { get; }
    }
}
