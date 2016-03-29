// Copyright 2016 door Erik de Roos
using System;

namespace RemoteGenerator.Builder.Wachtrij
{
    class BestandStreamToken
    {
        public Guid ID { get; set; }
        public bool Ontvangen { get { return !string.IsNullOrEmpty(LinkOpFilesysteem); } }
        public string LinkOpFilesysteem { get; set; }
    }
}
