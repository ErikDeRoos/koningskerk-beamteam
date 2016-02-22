using ConnectTools.Berichten;
using System.Collections.Generic;

namespace RemoteGenerator.Builder
{
    interface IPpGenerator
    {
        IEnumerable<WachtrijRegel> Wachtrij { get; }
        IEnumerable<WachtrijRegel> Gereed { get; }
        WachtrijRegel NieuweWachtrijRegel(Instellingen gebruikInstellingen);
        void UpdateWachtrijRegel(Token voorToken, Liturgie liturgie);
    }
}
