using Generator.Database;
using Generator.Database.Models;
using Moq;
using System.Collections.Generic;

namespace Generator.Tests.Builders
{
    public class LiturgieDatabaseBuilder
    {
        public Mock<ILiturgieDatabase> Database { get; } = new Mock<ILiturgieDatabase>();

        public ILiturgieDatabase Build()
        {
            return Database.Object;
        }

        public LiturgieDatabaseBuilder KrijgItem_AddOnderdeelAndFragment(string onderdeelNaam, string fragmentNaam, string display = null, DatabaseZoekStatus status = DatabaseZoekStatus.Opgelost, string veiligeOnderdeelNaam = null, string veiligeFragmentNaam = null)
        {
            Database.Setup(x => x.KrijgItem(It.IsAny<VerwerkingType>(), onderdeelNaam, fragmentNaam, It.IsAny<IEnumerable<string>>(), It.IsAny<LiturgieSettings>()))
                .Returns(MockOplossing(onderdeelNaam, fragmentNaam, display, status, veiligeOnderdeelNaam ?? onderdeelNaam, veiligeFragmentNaam ?? fragmentNaam));

            return this;
        }

        private static IOplossing MockOplossing(string onderdeel, string fragment, string display, DatabaseZoekStatus status, string onderdeelVeilig, string fragmentVeilig)
        {
            var oplossing = new Mock<IOplossing>();
            oplossing.SetupGet(x => x.Onderdeel).Returns(new OplossingOnderdeel { Naam = onderdeel, VeiligeNaam = onderdeelVeilig, AlternatieveNaam = display });
            oplossing.SetupGet(x => x.Fragment).Returns(new OplossingOnderdeel { Naam = fragment, VeiligeNaam = fragmentVeilig });
            oplossing.SetupGet(x => x.Status).Returns(status);
            return oplossing.Object;
        }
    }
}
