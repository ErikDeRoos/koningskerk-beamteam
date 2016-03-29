// Copyright 2016 door Erik de Roos
using ConnectTools.Berichten;
using System.IO;
using System.ServiceModel;

namespace ConnectTools
{
    [ServiceContract]
    public interface IWCFServer
    {
        [OperationContract]
        Token StartConnectie(Instellingen gebruikInstellingen, Liturgie metLiturgie);

        [OperationContract]
        void ToevoegenBestand(SendFile file);

        [OperationContract]
        Voortgang StartGenereren(Token token);

        [OperationContract]
        Voortgang CheckVoortgang(Token token);

        [OperationContract]
        Stream DownloadResultaat(Token token);
    }
}
