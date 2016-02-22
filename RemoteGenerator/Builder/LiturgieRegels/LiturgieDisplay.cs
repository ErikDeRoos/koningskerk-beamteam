using ILiturgieDatabase;

namespace RemoteGenerator.Builder.LiturgieRegels
{
    class LiturgieDisplay : ILiturgieDisplay
    {

        public string Naam { get; set; }

        public string NaamOverzicht { get; set; }

        public string SubNaam { get; set; }

        public bool VersenAfleiden { get; set; }

        public string VersenDefault { get; set; }

        public LiturgieDisplay(ConnectTools.Berichten.LiturgieRegelDisplay vanDisplay)
        {
            Naam = vanDisplay.Naam;
            NaamOverzicht = vanDisplay.NaamOverzicht;
            SubNaam = vanDisplay.SubNaam;
            VersenAfleiden = vanDisplay.VersenAfleiden;
            VersenDefault = vanDisplay.VersenDefault;
        }
    }
}
