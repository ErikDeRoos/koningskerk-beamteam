// Copyright 2019 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase {
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
