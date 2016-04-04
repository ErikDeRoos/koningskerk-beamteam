// Copyright 2016 door Erik de Roos

namespace ILiturgieDatabase
{
    public enum LiturgieOplossingResultaat
    {
        Opgelost = 0,
        DatabaseFout = 1,
        SetFout = 2,
        SubSetFout = 3,
        VersFout = 4,  // Vers niet gevonden
        VersOnderverdelingMismatch = 5,  // Als er wel versen gevraagd worden maar de set geen versen ondersteund
        VersOnleesbaar = 6,  // Vers data type probleem
    }

}
