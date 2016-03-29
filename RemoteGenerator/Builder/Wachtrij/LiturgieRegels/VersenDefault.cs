// Copyright 2016 door Erik de Roos
using ILiturgieDatabase;

namespace RemoteGenerator.Builder.Wachtrij.LiturgieRegels
{
    class VersenDefault : IVersenDefault
    {
        public bool Gebruik { get; set; }
        public string Tekst { get; set; }

        public VersenDefault(ConnectTools.Berichten.VerzenDefault vanDefault)
        {
            Gebruik = vanDefault.Gebruik;
            Tekst = vanDefault.Tekst;
        }
    }
}
