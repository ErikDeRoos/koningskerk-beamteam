using System.Collections.Generic;

namespace ISettings
{
    public interface IInstellingen
    {
        string Databasepad { get; }
        string Templateliederen { get; }
        string Templatetheme { get; }
        int Regelsperslide { get; }
        IStandaardTeksten StandaardTeksten { get; }

        IEnumerable<IMapmask> Masks { get; }

        bool AddMask(IMapmask mask);
        void ClearMasks();

        string FullDatabasePath { get; }

        string FullTemplatetheme { get; }

        string FullTemplateliederen { get; }

        string ToString();
    }
}
