// Copyright 2016 door Erik de Roos
using System;
using System.IO;

namespace Tools
{
    public class FoutmeldingSchrijver
    {
        public static void Log(Exception melding)
        {
            Log(melding.ToString());
        }
        public static void Log(string melding)
        {
            using (var sw = new StreamWriter("ppgenerator.log", true))
            {
                sw.WriteLine($"[{DateTime.Now}] Foutmelding door [{Environment.UserName}] op systeem [{Environment.MachineName}]");
                sw.WriteLine(melding);
            }
        }
    }
}
