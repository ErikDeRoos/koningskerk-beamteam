// Copyright 2019 door Erik de Roos
using Autofac;

namespace PowerpointGenerator
{
    static class Bootstrap
    {
        private const string SettingsFileName = @"lib\instellingen.json";
        private const string MasksFileName = @"lib\masks.json";
        private const string DefaultSetNameEmpty = "!leeg";

        internal static void SetDefault(ContainerBuilder container)
        {
            container.RegisterType<Tools.LocalFileOperations>().As<IFileSystem.IFileOperations>();
            container.RegisterType<Settings.SettingsFactory>().As<ISettings.IInstellingenFactory>()
                .WithParameter("instellingenFileName", SettingsFileName).WithParameter("masksFileName", MasksFileName)
                .SingleInstance();
            container.RegisterGeneric(typeof(Generator.GeneratieInterface<>));
            container.RegisterType<Screens.CompRegistration>().As<Generator.ICompRegistration>();
            SetGenerator(container);
            container.RegisterType<Screens.Form1>().As<System.Windows.Forms.Form>()
                .OnActivated(f => f.Instance.Opstarten());
        }

        private static void SetGenerator(ContainerBuilder container)
        {
            container.RegisterType<Generator.Database.LiturgieDatabase>().As<ILiturgieDatabase.ILiturgieDatabase>();
            container.RegisterType<Generator.LiturgieOplosser.LiturgieOplosser>().As<ILiturgieDatabase.ILiturgieSlideMaker>()
                .WithParameter("defaultSetNameEmpty", DefaultSetNameEmpty)
                .InstancePerLifetimeScope();
            container.RegisterType<Generator.LiturgieOplosser.LiturgieZoeker>().As<ILiturgieDatabase.ILiturgieZoeken>()
                .InstancePerLifetimeScope();
            container.RegisterType<mppt.RegelVerwerking.LengteBerekenaar>().As<ILiturgieDatabase.ILengteBerekenaar>();
            container.RegisterType<Generator.LiturgieInterpretator.LiturgieTekstNaarObject>().As<ILiturgieDatabase.ILiturgieTekstNaarObject>();
            container.RegisterType<Generator.Database.FileSystem.FileEngine>().As<IDatabase.Engine.IEngine>();
            container.RegisterType<Database.EngineManager>().As<IDatabase.IEngineManager>();
            SetMsPowerpointBuilder(container);
        }

        private static void SetMsPowerpointBuilder(ContainerBuilder container)
        {
            container.RegisterType<mppt.PowerpointFunctions>().As<ISlideBuilder.IBuilder>();
            container.RegisterType<mppt.Connect.MppFactory>().As<mppt.Connect.IMppFactory>();
            container.RegisterType<mppt.Connect.MppApplication>().As<mppt.Connect.IMppApplication>();
            container.RegisterType<mppt.LiedPresentator.LiedFormatter>().As<mppt.LiedPresentator.ILiedFormatter>();
            container.RegisterType<Settings.MicrosoftPowerpointWrapperSettings>().As<mppt.ISettings>();
        }
    }
}
