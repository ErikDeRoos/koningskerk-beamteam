// Copyright 2017 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase {
    public interface ILiturgieInterpreteer
    {
        ILiturgieInterpretatie VanTekstregel(string regels);
        IEnumerable<ILiturgieInterpretatie> VanTekstregels(string[] regels);
        ILiturgieOptiesGebruiker BepaalBasisOptiesTekstinvoer(string invoerTekst, string uitDatabase);
        string MaakTekstVanOpties(ILiturgieOptiesGebruiker opties);
        string[] SplitsVoorOpties(string liturgieRegel);
        ILiturgieOptiesGebruiker BepaalOptiesTekstinvoer(string optiesTekst);
    }
}
