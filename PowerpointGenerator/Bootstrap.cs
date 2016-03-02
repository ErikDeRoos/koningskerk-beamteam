using Autofac;
using System;

namespace PowerpointGenerator
{
    static class Bootstrap
    {
        internal static void SetDefault(ContainerBuilder container)
        {
            container.RegisterType<Database.LiturgieDatabase>().As<ILiturgieDatabase.ILiturgieLosOp>();
            container.RegisterGeneric(typeof(Database.FileEngine<>)).As(typeof(IDatabase.IEngine<>));
            container.RegisterType<mppt.PowerpointFunctions>().As<ISlideBuilder.IBuilder>();
            container.RegisterType<Tools.LocalFileOperations>().As<IFileSystem.IFileOperations>();
            container.RegisterType<Settings.SettingsFactory>().As<ISettings.IInstellingenFactory>().WithParameter("instellingenFileName", "instellingen.xml").WithParameter("masksFileName", "masks.xml");
            container.RegisterType<Form1>().As<System.Windows.Forms.Form>().OnActivated(f => f.Instance.Opstarten());
        }
    }
}
