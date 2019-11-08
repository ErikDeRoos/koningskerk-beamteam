using ILiturgieDatabase;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Generator.Tests.Builders
{
    public class LiturgieDatabaseZoekBuilder
    {
        public Mock<ILiturgieDatabaseZoek> Database { get; } = new Mock<ILiturgieDatabaseZoek>();

        private List<IZoekresultaat> _zoekresultaten = new List<IZoekresultaat>();
        private Dictionary<string, List<IZoekresultaat>> _fragmentZoekresultaten = new Dictionary<string, List<IZoekresultaat>>();

        public ILiturgieDatabaseZoek Build()
        {
            Database.Setup(x => x.KrijgAlleSetNamenInNormaleDb())
                .Returns(_zoekresultaten);
            foreach(var fragmentGroup in _fragmentZoekresultaten)
                Database.Setup(x => x.KrijgAlleFragmentenUitNormaleDb(fragmentGroup.Key))
                    .Returns(fragmentGroup.Value);

            return Database.Object;
        }

        public LiturgieDatabaseZoekBuilder KrijgAlleSetNamenInNormaleDb_AddOnderdeel(string onderdeelNaam, string veiligeOnderdeelNaam = null)
        {
            veiligeOnderdeelNaam = veiligeOnderdeelNaam ?? onderdeelNaam;
            if (_zoekresultaten.Any(z => z.Resultaat.VeiligeNaam == veiligeOnderdeelNaam))
                return this;  // Onderdelen maar 1x toevoegen
            _zoekresultaten.Add(MockZoekresultaat(onderdeelNaam, veiligeOnderdeelNaam));

            return this;
        }

        public LiturgieDatabaseZoekBuilder KrijgAlleFragmentenUitSet_AddFragment(string onderdeelNaam, string fragmentNaam, string veiligeOnderdeelNaam = null, string veiligeFragmentNaam = null)
        {
            veiligeOnderdeelNaam = veiligeOnderdeelNaam ?? onderdeelNaam;
            veiligeFragmentNaam = veiligeFragmentNaam ?? fragmentNaam;

            if (!_fragmentZoekresultaten.ContainsKey(onderdeelNaam))
                _fragmentZoekresultaten.Add(onderdeelNaam, new List<IZoekresultaat>());

            var list = _fragmentZoekresultaten[onderdeelNaam];
            var naam = $"{onderdeelNaam} {fragmentNaam}";
            var veiligeNaam = $"{veiligeOnderdeelNaam} {veiligeFragmentNaam}";

            if (list.Any(z => z.Resultaat.VeiligeNaam == veiligeNaam))
                return this;  // Onderdelen maar 1x toevoegen
            list.Add(MockZoekresultaat(naam, veiligeNaam));

            return this;
        }

        private static IZoekresultaat MockZoekresultaat(string onderdeel, string onderdeelVeilig)
        {
            var bron = new Mock<IZoekresultaatBron>();
            bron.SetupGet(x => x.Weergave).Returns("database");

            var entry = new Mock<IZoekresultaatEntry>();
            entry.SetupGet(x => x.Weergave).Returns(onderdeel);
            entry.SetupGet(x => x.VeiligeNaam).Returns(onderdeelVeilig);

            var oplossing = new Mock<IZoekresultaat>();
            oplossing.SetupGet(x => x.Database).Returns(bron.Object);
            oplossing.SetupGet(x => x.Resultaat).Returns(entry.Object);

            return oplossing.Object;
        }
    }
}
