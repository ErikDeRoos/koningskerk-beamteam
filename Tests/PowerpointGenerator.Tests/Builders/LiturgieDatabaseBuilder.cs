using ILiturgieDatabase;
using Moq;
using System.Collections.Generic;

namespace Generator.Tests.Builders
{
    public class LiturgieDatabaseBuilder
    {
        public Mock<ILiturgieDatabase.ILiturgieDatabase> Database { get; } = new Mock<ILiturgieDatabase.ILiturgieDatabase>();

        public ILiturgieDatabase.ILiturgieDatabase Build()
        {
            return Database.Object;
        }

        public LiturgieDatabaseBuilder ZoekSpecifiek_AddOnderdeelAndFragment(string onderdeelNaam, string fragmentNaam, string display = null, LiturgieOplossingResultaat status = LiturgieOplossingResultaat.Opgelost)
        {
            Database.Setup(x => x.ZoekSpecifiek(It.IsAny<ILiturgieDatabase.VerwerkingType>(), onderdeelNaam, fragmentNaam, It.IsAny<IEnumerable<string>>(), It.IsAny<ILiturgieDatabase.LiturgieSettings>()))
                .Returns(MockOplossing(onderdeelNaam, fragmentNaam, display, status));

            return this;
        }

        private static ILiturgieDatabase.IOplossing MockOplossing(string onderdeel, string fragment, string display, LiturgieOplossingResultaat status)
        {
            var oplossing = new Mock<IOplossing>();
            oplossing.SetupGet(x => x.Onderdeel).Returns(new OplossingOnderdeel { OrigineleNaam = onderdeel, VeiligeNaam = onderdeel, DisplayNaam = display });
            oplossing.SetupGet(x => x.Fragment).Returns(new OplossingOnderdeel { OrigineleNaam = fragment, VeiligeNaam = fragment });
            oplossing.SetupGet(x => x.Status).Returns(status);
            return oplossing.Object;
        }
    }
}
