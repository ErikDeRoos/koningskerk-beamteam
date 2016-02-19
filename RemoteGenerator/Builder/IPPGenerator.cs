using ConnectTools.Berichten;
using System.Collections.Generic;

namespace RemoteGenerator.Builder
{
    interface IPpGenerator
    {
        IEnumerable<WachtrijRegel> Wachtrij { get; }
        WachtrijRegel NieuweWachtrijRegel(Liturgie opBasisVanLiturgie);
    }
}
