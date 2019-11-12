// Copyright 2019 door Erik de Roos
namespace Generator.LiturgieInterpretator.Models
{

    public class LiturgieOptiesGebruiker
    {
        public bool NietVerwerkenViaDatabase { get; set; }
        public bool? ToonInOverzicht { get; set; }
        public bool? ToonInVolgende { get; set; }
        public bool AlsBijbeltekst { get; set; }
        public string AlternatieveNaamOverzicht { get; set; }
        public string AlternatieveNaam { get; set; }
    }
}
