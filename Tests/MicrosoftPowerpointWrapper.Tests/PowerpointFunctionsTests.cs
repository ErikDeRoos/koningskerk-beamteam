﻿// Copyright 2016 door Erik de Roos
using Autofac.Extras.FakeItEasy;
using FakeItEasy;
using ISlideBuilder;
using mppt.Connect;
using NUnit.Framework;
using System.Collections.Generic;

namespace MicrosoftPowerpointWrapper.Tests
{
    public class PowerpointFunctionsTests
    {
        [OneTimeSetUp]
        public void Initialise()
        {
        }

        [TestFixture]
        public class GeneratePresentationMethod : PowerpointFunctionsTests
        {
            [Test]
            public void Application_Opened()
            {
                using (var fake = new AutoFake())
                {
                    var app = fake.Resolve<IMppApplication>();
                    A.CallTo(() => fake.Resolve<IMppFactory>().GetApplication()).Returns(app);
                    var sut = fake.Resolve<mppt.PowerpointFunctions>();
                    var dependendFiles = A.Fake<IBuilderDependendFiles>();
                    A.CallTo(() => dependendFiles.FullTemplateTheme).Returns("\testbestand.ppt");
                    sut.PreparePresentation(GetEmptyLiturgie(), A.Fake<IBuilderBuildSettings>(), A.Fake<IBuilderBuildDefaults>(), dependendFiles, null);

                    sut.GeneratePresentation();

                    A.CallTo(() => app.Open(dependendFiles.FullTemplateTheme, true)).MustHaveHappened();
                }
            }

            [TestCase("presentatie.pptx")]
            public void Application_Saved(string saveAsFileName)
            {
                using (var fake = new AutoFake())
                {
                    var dependendFiles = A.Fake<IBuilderDependendFiles>();
                    A.CallTo(() => dependendFiles.FullTemplateTheme).Returns("\testbestand.ppt");
                    var pres = PreparePresentation(fake, dependendFiles.FullTemplateTheme);
                    var sut = fake.Resolve<mppt.PowerpointFunctions>();
                    sut.PreparePresentation(GetEmptyLiturgie(), A.Fake<IBuilderBuildSettings>(), A.Fake<IBuilderBuildDefaults>(), dependendFiles, saveAsFileName);

                    sut.GeneratePresentation();

                    A.CallTo(() => pres.OpslaanAls(saveAsFileName)).MustHaveHappened();
                }
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