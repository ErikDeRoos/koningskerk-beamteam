using System;
using System.Collections.Generic;
using ISettings;
using ILiturgieDatabase;

namespace ISlideBuilder
{
    public interface IBuilder : IDisposable
    {
        Action<int, int, int> Voortgang { set; }
        Action<Status, string, int?> StatusWijziging { set; }

        void PreparePresentation(IEnumerable<ILiturgieRegel> liturgie, string voorganger, string collecte1, string collecte2, string lezen, string tekst, IInstellingen gebruikInstellingen, string opslaanAls);
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