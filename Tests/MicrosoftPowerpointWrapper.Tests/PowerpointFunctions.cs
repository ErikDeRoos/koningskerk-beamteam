using Autofac.Extras.FakeItEasy;
using FakeItEasy;
using mppt.Connect;
using NUnit.Framework;
using System.Collections.Generic;

namespace MicrosoftPowerpointWrapper.Tests
{
    public class PowerpointFunctions
    {
        public ISettings.IInstellingen DefaultInstellingen = new ISettings.CommonImplementation.Instellingen();

        [Test]
        public void GeneratePresentation_Application_Opened()
        {
            using (var fake = new AutoFake())
            {
                var app = fake.Resolve<IMppApplication>();
                A.CallTo(() => fake.Resolve<IMppFactory>().GetApplication()).Returns(app);
                var sut = fake.Resolve<mppt.PowerpointFunctions>();
                sut.PreparePresentation(GetEmptyLiturgie(), null, null, null, null, null, DefaultInstellingen, null);

                sut.GeneratePresentation();

                A.CallTo(() => app.Open(DefaultInstellingen.FullTemplatetheme, true)).MustHaveHappened();
            }
        }

        [TestCase("presentatie.pptx")]
        public void GeneratePresentation_Application_Saved(string saveAsFileName)
        {
            using (var fake = new AutoFake())
            {
                var pres = PreparePresentation(fake, DefaultInstellingen.FullTemplatetheme);
                var sut = fake.Resolve<mppt.PowerpointFunctions>();
                sut.PreparePresentation(GetEmptyLiturgie(), null, null, null, null, null, DefaultInstellingen, saveAsFileName);

                sut.GeneratePresentation();

                A.CallTo(() => pres.OpslaanAls(saveAsFileName)).MustHaveHappened();
            }
        }

        private static IMppPresentatie PreparePresentation(AutoFake fakeScope, string fileName)
        {
            var app = fakeScope.Resolve<IMppApplication>();
            A.CallTo(() => fakeScope.Resolve<IMppFactory>().GetApplication()).Returns(app);
            var pres = fakeScope.Resolve<IMppPresentatie>();
            A.CallTo(() => app.Open(fileName, true)).Returns(pres);
            return pres;
        }

        private static IEnumerable<ILiturgieDatabase.ILiturgieRegel> GetEmptyLiturgie()
        {
            return new List<ILiturgieDatabase.ILiturgieRegel>();
        }
    }
}
