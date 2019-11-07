using ILiturgieDatabase;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Generator.Tests.Builders
{
    public class LiturgieDatabaseBuilder
    {
        public Mock<ILiturgieDatabase.ILiturgieDatabase> Database { get; } = new Mock<ILiturgieDatabase.ILiturgieDatabase>();

        private List<IZoekresultaat> _zoekresultaten = new List<IZoekresultaat>();
        private Dictionary<string, List<IZoekresultaat>> _fragmentZoekresultaten = new Dictionary<string, List<IZoekresultaat>>();

        public ILiturgieDatabase.ILiturgieDatabase Build()
        {
            Database.Setup(x => x.KrijgAlleSetNamenInNormaleDb())
                .Returns(_zoekresultaten);
            foreach(var fragmentGroup in _fragmentZoekresultaten)
                Database.Setup(x => x.KrijgAlleFragmentenUitSet(fragmentGroup.Key))
                    .Returns(fragmentGroup.Value);

            return Database.Object;
        }

        public LiturgieDatabaseBuilder ZoekSpecifiek_AddOnderdeelAndFragment(string onderdeelNaam, string fragmentNaam, string display = null, DatabaseZoekStatus status = DatabaseZoekStatus.Opgelost, string veiligeOnderdeelNaam = null, string veiligeFragmentNaam = null)
        {
            Database.Setup(x => x.ZoekSpecifiekItem(It.IsAny<ILiturgieDatabase.VerwerkingType>(), onderdeelNaam, fragmentNaam, It.IsAny<IEnumerable<string>>(), It.IsAny<ILiturgieDatabase.LiturgieSettings>()))
                .Returns(MockOplossing(onderdeelNaam, fragmentNaam, display, status, veiligeOnderdeelNaam ?? onderdeelNaam, veiligeFragmentNaam ?? fragmentNaam));

            return this;
        }

        public LiturgieDatabaseBuilder KrijgAlleSetNamenInNormaleDb_AddOnderdeel(string onderdeelNaam, string veiligeOnderdeelNaam = null)
        {
            veiligeOnderdeelNaam = veiligeOnderdeelNaam ?? onderdeelNaam;
            if (_zoekresultaten.Any(z => z.Resultaat.VeiligeNaam == veiligeOnderdeelNaam))
                return this;  // Onderdelen maar 1x toevoegen
            _zoekresultaten.Add(MockZoekresultaat(onderdeelNaam, veiligeOnderdeelNaam));

            return this;
        }

        public LiturgieDatabaseBuilder KrijgAlleFragmentenUitSet_AddFragment(string onderdeelNaam, string fragmentNaam, string veiligeOnderdeelNaam = null, string veiligeFragmentNaam = null)
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

        private static IOplossing MockOplossing(string onderdeel, string fragment, string display, DatabaseZoekStatus status, string onderdeelVeilig, string fragmentVeilig)
        {
            var oplossing = new Mock<IOplossing>();
            oplossing.SetupGet(x => x.Onderdeel).Returns(new OplossingOnderdeel { OrigineleNaam = onderdeel, VeiligeNaam = onderdeelVeilig, DisplayNaam = display });
            oplossing.SetupGet(x => x.Fragment).Returns(new OplossingOnderdeel { OrigineleNaam = fragment, VeiligeNaam = fragmentVeilig });
            oplossing.SetupGet(x => x.Status).Returns(status);
            return oplossing.Object;
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
