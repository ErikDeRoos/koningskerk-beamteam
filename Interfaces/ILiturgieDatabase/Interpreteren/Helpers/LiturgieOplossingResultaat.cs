// Copyright 2016 door Erik de Roos

namespace ILiturgieDatabase
{
    public enum LiturgieOplossingResultaat
    {
        Onbekend = 0,
        Opgelost = 1,
        DatabaseFout = 2,
        SetFout = 3,
        SubSetFout = 4,
        VersFout = 5,  // Vers niet gevonden
        VersOnderverdelingMismatch = 6,  // Als er wel versen gevraagd worden maar de set geen versen ondersteund
        VersOnleesbaar = 7,  // Vers data type probleem
    }

}
