// Copyright 2016 door Remco Veurink en Erik de Roos
using ILiturgieDatabase;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PowerpointGenerator
{
    public partial class LiturgieNotFoundFormulier : Form
    {
        public LiturgieNotFoundFormulier(IEnumerable<ILiturgieOplossing> fouten)
        {
            InitializeComponent();
            textBox1.Lines = fouten
                .Select(l => $"{l.VanInterpretatie.Benaming} {l.VanInterpretatie.Deel}: {KrijgHelpendeTekstBijLiturgieFout(l.Resultaat, l.VanInterpretatie)}")
                .ToArray();
        }

        public static string KrijgHelpendeTekstBijLiturgieFout(LiturgieOplossingResultaat resultaatFout, ILiturgieInterpretatie vanInterpretatie)
        {
            switch (resultaatFout)
            {
                case LiturgieOplossingResultaat.DatabaseFout:
                    return $"De tekst achter 'als' is niet correct.";
                case LiturgieOplossingResultaat.SetFout:
                    return $"'{vanInterpretatie.Benaming}' is niet een bekend lied, bijbelboek of slide.";
                case LiturgieOplossingResultaat.SubSetFout:
                    return string.IsNullOrWhiteSpace(vanInterpretatie.Deel) ? "Je moet een hoofdstuk of lied opgeven." : $"'{vanInterpretatie.Deel}' is niet te vinden in {vanInterpretatie.Benaming}.";
                case LiturgieOplossingResultaat.VersFout:
                    return $"Niet alle verzen konden gevonden worden in de database.";
                case LiturgieOplossingResultaat.VersOnderverdelingMismatch:
                    return $"Bij '{vanInterpretatie.Deel} {vanInterpretatie.Deel}' kan je helaas geen specifieke verzen opgeven.";
                case LiturgieOplossingResultaat.VersOnleesbaar:
                    return $"Een van de verzen geeft aan dat er geen inhoud aan hem gekoppeld is in de database. Kan je niet echt iets mee, maar dat gaat nu dus wel fout.";
                case LiturgieOplossingResultaat.Onbekend:
                    return "Er is iets goed mis, ik kan je niet verder helpen.";
            }
            return string.Empty;
        }
    }
}
