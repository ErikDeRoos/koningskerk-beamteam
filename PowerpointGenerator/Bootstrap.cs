// Copyright 2019 door Erik de Roos
using Autofac;
using Generator.Database;
using Generator.Database.FileSystem;
using Generator.LiturgieInterpretator;
using Generator.LiturgieInterpretator.Models;
using Generator.Tools;
using mppt;
using PowerpointGenerator.Genereren;

namespace PowerpointGenerator
{
    static class Bootstrap
    {
        private const string SettingsFileName = @"lib\instellingen.json";
        private const string MasksFileName = @"lib\masks.json";
        private const string DefaultSetNameEmpty = "!leeg";

        internal static void SetDefault(ContainerBuilder container)
        {
            container.RegisterType<LocalFileOperations>().As<IFileOperations>();
            container.RegisterType<Settings.SettingsFactory>().As<ISettings.IInstellingenFactory>()
                .WithParameter("instellingenFileName", SettingsFileName).WithParameter("masksFileName", MasksFileName)
                .SingleInstance();
            container.RegisterGeneric(typeof(GeneratieInterface<>));
            container.RegisterType<Screens.CompRegistration>().As<ICompRegistration>();
            SetGenerator(container);
            container.RegisterType<Screens.Form1>().As<System.Windows.Forms.Form>()
                .OnActivated(f => f.Instance.Opstarten());
            SetMsPowerpointBuilder(container);
        }

        private static void SetGenerator(ContainerBuilder container)
        {
            container.RegisterType<LiturgieDatabase>().As<ILiturgieDatabase>();
            container.RegisterType<LiturgieDatabaseZoek>().As<ILiturgieDatabaseZoek>();
            container.RegisterType<LiturgieOplosser>().As<ILiturgieSlideMaker>()
                .WithParameter("defaultSetNameEmpty", DefaultSetNameEmpty);
            container.RegisterType<LiturgieZoeker>().As<ILiturgieZoeken>();
            container.RegisterType<mppt.RegelVerwerking.LengteBerekenaar>().As<ILengteBerekenaar>();
            container.RegisterType<LiturgieTekstNaarObject>().As<ILiturgieTekstNaarObject>();
            container.RegisterType<FileEngine>().As<IEngine>();
            container.RegisterType<EngineManager>().As<IEngineManager>()
                .InstancePerLifetimeScope();
        }

        private static void SetMsPowerpointBuilder(ContainerBuilder container)
        {
            container.RegisterType<mppt.PowerpointFunctions>().As<IBuilder>();
            container.RegisterType<mppt.Connect.MppFactory>().As<mppt.Connect.IMppFactory>();
            container.RegisterType<mppt.Connect.MppApplication>().As<mppt.Connect.IMppApplication>();
            container.RegisterType<mppt.LiedPresentator.LiedFormatter>().As<mppt.LiedPresentator.ILiedFormatter>();
            container.RegisterType<Settings.MicrosoftPowerpointWrapperSettings>().As<mppt.ISettings>();
        }
    }
}
