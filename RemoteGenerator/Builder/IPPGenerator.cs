using ConnectTools.Berichten;
using System;
using System.Collections.Generic;
using System.IO;

namespace RemoteGenerator.Builder
{
    interface IPpGenerator
    {
        IEnumerable<WachtrijRegel> Wachtrij { get; }
        IEnumerable<WachtrijRegel> Verwerkt { get; }
        WachtrijRegel NieuweWachtrijRegel(Instellingen gebruikInstellingen, Liturgie metLiturgie);
        void UpdateWachtrijRegel(Token voorToken, Guid bestandToken, Stream toevoegenBestand);
        Voortgang ProbeerTeStarten(Token voorToken);
    }
}
