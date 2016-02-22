using ConnectTools.Berichten;
using System.ServiceModel;

namespace RemoteGenerator.WCF
{
    [ServiceContract]
    public interface IWCFServer
    {
        [OperationContract]
        Token StartConnectie(Instellingen gebruikInstellingen);

        [OperationContract]
        void StartGenereren(Token token, Liturgie metLiturgie);
        [OperationContract]
        Voortgang CheckVoortgang(Token token);

        [OperationContract]
        byte[] DownloadResultaat(Token token);
    }
}
