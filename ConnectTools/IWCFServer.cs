using ConnectTools.Berichten;
using System.IO;
using System.ServiceModel;

namespace ConnectTools
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
        Stream DownloadResultaat(Token token);
    }
}
