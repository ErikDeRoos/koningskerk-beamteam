using System;
using System.Collections.Generic;
using IDatabase;
using ISettings;

namespace ISlideBuilder
{
    public interface IBuilder : IDisposable
    {
        Action<int, int, int> Voortgang { set; }
        Action<Status, string> StatusWijziging { set; }

        void PreparePresentation(IEnumerable<ILiturgieZoekresultaat> liturgie, string Voorganger, string Collecte1, string Collecte2, string Lezen, string Tekst, IInstellingen gebruikInstellingen, string opslaanAls);
        void GeneratePresentation();
        void Stop();
    }

    public enum Status
    {
        Gestart,
        StopFout,
        StopGoed,
    }

}