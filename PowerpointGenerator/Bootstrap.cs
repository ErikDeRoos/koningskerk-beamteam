// Copyright 2017 door Erik de Roos
using Autofac;

namespace PowerpointGenerator
{
    static class Bootstrap
    {
        internal static void SetDefault(ContainerBuilder container)
        {
            container.RegisterType<Tools.LocalFileOperations>().As<IFileSystem.IFileOperations>();
            container.RegisterType<Settings.SettingsFactory>().As<ISettings.IInstellingenFactory>()
                .WithParameter("instellingenFileName", Properties.Settings.Default.InstellingenFileName).WithParameter("masksFileName", Properties.Settings.Default.MasksFileName)
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
            container.RegisterType<Generator.LiturgieOplosser.LiturgieOplosser>().As<ILiturgieDatabase.ILiturgieLosOp>()
                .WithParameter("defaultSetNameEmpty", Properties.Settings.Default.SetNameEmpty);
            container.RegisterType<mppt.RegelVerwerking.LengteBerekenaar>().As<ILiturgieDatabase.ILengteBerekenaar>();
            container.RegisterGeneric(typeof(Generator.Database.FileSystem.FileEngine<>)).As(typeof(IDatabase.Engine.IEngine<>));
            container.RegisterGeneric(typeof(Database.EngineManager<>)).As(typeof(IDatabase.IEngineManager<>));
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
