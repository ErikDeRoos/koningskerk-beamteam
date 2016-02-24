
namespace ILiturgieDatabase
{
    public enum LiturgieOplossingResultaat
    {
        Opgelost = 0,
        SetFout = 1,
        SubSetFout = 2,
        VersFout = 3,  // Vers niet gevonden
        VersOnderverdelingMismatch = 4,  // Als er wel versen gevraagd worden maar de set geen versen ondersteund
        VersOnleesbaar = 5,  // Vers data type probleem
    }

}
