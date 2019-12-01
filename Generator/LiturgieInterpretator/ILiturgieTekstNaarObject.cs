// Copyright 2019 door Erik de Roos
using Generator.LiturgieInterpretator.Models;
using System.Collections.Generic;

namespace Generator.LiturgieInterpretator
{
    public interface ILiturgieTekstNaarObject
    {
        ILiturgieTekstObject VanTekstregel(string regels);
        IEnumerable<ILiturgieTekstObject> VanTekstregels(string[] regels);
        LiturgieOptiesGebruiker BepaalBasisOptiesTekstinvoer(string invoerTekst, string uitDatabase);
        string MaakTekstVanOpties(LiturgieOptiesGebruiker opties);
        string[] SplitsVoorOpties(string liturgieRegel);
        LiturgieOptiesGebruiker BepaalOptiesTekstinvoer(string optiesTekst);
    }
}
