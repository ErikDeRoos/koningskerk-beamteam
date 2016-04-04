﻿// Copyright 2016 door Erik de Roos
using ILiturgieDatabase;

namespace RemoteGenerator.Builder.Wachtrij.LiturgieRegels
{
    class LiturgieDisplay : ILiturgieDisplay
    {

        public string Naam { get; set; }

        public string NaamOverzicht { get; set; }

        public string SubNaam { get; set; }

        public bool VolledigeContent { get; set; }

        public IVersenDefault VersenGebruikDefault { get; set; }

        public LiturgieDisplay(ConnectTools.Berichten.LiturgieRegelDisplay vanDisplay)
        {
            Naam = vanDisplay.Naam;
            NaamOverzicht = vanDisplay.NaamOverzicht;
            SubNaam = vanDisplay.SubNaam;
            VolledigeContent = vanDisplay.VolledigeContent;
            VersenGebruikDefault = vanDisplay.VersenGebruikDefault != null ? new VersenDefault(vanDisplay.VersenGebruikDefault) : null;
        }
    }
}
