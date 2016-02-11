using System;
using System.IO;

namespace PowerpointGenerater
{
    class FoutmeldingSchrijver
    {
        public static void Log(Exception melding)
        {
            Log(melding.ToString());
        }
        public static void Log(String melding)
        {
            using (var sw = new StreamWriter("ppgenerator.log", true))
            {
                sw.WriteLine(String.Format("[{0}] Foutmelding door [{1}] op systeem [{2}]", DateTime.Now, Environment.UserName, Environment.MachineName));
                sw.WriteLine(melding);
            }
        }
    }
}
