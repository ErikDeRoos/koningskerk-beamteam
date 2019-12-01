// Copyright 2016 door Remco Veurink en Erik de Roos
using Generator.Database.Models;
using Generator.LiturgieInterpretator.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PowerpointGenerator
{
    public partial class LiturgieNotFoundFormulier : Form
    {
        public LiturgieNotFoundFormulier(IEnumerable<ITekstNaarSlideConversieResultaat> fouten)
        {
            InitializeComponent();
            textBox1.Lines = fouten
                .Select(l => $"{l.InputTekst.Benaming} {l.InputTekst.Deel}: {KrijgHelpendeTekstBijLiturgieFout(l.ResultaatStatus, l.InputTekst)}")
                .ToArray();
        }

        public static string KrijgHelpendeTekstBijLiturgieFout(DatabaseZoekStatus resultaatFout, ILiturgieTekstObject vanInterpretatie)
        {
            switch (resultaatFout)
            {
                case DatabaseZoekStatus.DatabaseFout:
                    return $"De tekst achter 'als' is niet correct.";
                case DatabaseZoekStatus.SetFout:
                    return $"'{vanInterpretatie.Benaming}' is niet een bekend lied, bijbelboek of slide.";
                case DatabaseZoekStatus.SubSetFout:
                    return string.IsNullOrWhiteSpace(vanInterpretatie.Deel) ? "Je moet een hoofdstuk of lied opgeven." : $"'{vanInterpretatie.Deel}' is niet te vinden in {vanInterpretatie.Benaming}.";
                case DatabaseZoekStatus.VersFout:
                    return $"Niet alle verzen konden gevonden worden in de database.";
                case DatabaseZoekStatus.VersOnderverdelingMismatch:
                    return $"Bij '{vanInterpretatie.Deel} {vanInterpretatie.Deel}' kan je helaas geen specifieke verzen opgeven.";
                case DatabaseZoekStatus.VersOnleesbaar:
                    return $"Een van de verzen geeft aan dat er geen inhoud aan hem gekoppeld is in de database. Kan je niet echt iets mee, maar dat gaat nu dus wel fout.";
                case DatabaseZoekStatus.Onbekend:
                    return "Er is iets goed mis, ik kan je niet verder helpen.";
            }
            return string.Empty;
        }
    }
}
