using Autofac;

namespace PowerpointGenerator
{
    static class Bootstrap
    {
        internal static void SetDefault(ContainerBuilder container)
        {
            container.RegisterType<Database.LiturgieDatabase>().As<ILiturgieDatabase.ILiturgieDatabase>();
            container.RegisterType<LiturgieOplosser.LiturgieOplosser>().As<ILiturgieDatabase.ILiturgieLosOp>();
            container.RegisterGeneric(typeof(Database.FileSystem.FileEngine<>)).As(typeof(IDatabase.IEngine<>));
            SetMsPowerpointBuilder(container);
            container.RegisterType<Tools.LocalFileOperations>().As<IFileSystem.IFileOperations>();
            container.RegisterType<Settings.SettingsFactory>().As<ISettings.IInstellingenFactory>().WithParameter("instellingenFileName", Properties.Settings.Default.InstellingenFileName).WithParameter("masksFileName", Properties.Settings.Default.MasksFileName);
            container.RegisterType<Form1>().As<System.Windows.Forms.Form>().OnActivated(f => f.Instance.Opstarten());
        }
        private static void SetMsPowerpointBuilder(ContainerBuilder container)
        {
            container.RegisterType<mppt.PowerpointFunctions>().As<ISlideBuilder.IBuilder>();
            container.RegisterType<mppt.Connect.MppFactory>().As<mppt.Connect.IMppFactory>();
            container.RegisterType<mppt.Connect.MppApplication>().As<mppt.Connect.IMppApplication>();
            container.RegisterType<mppt.LiedPresentator.LiedFormatter>().As<mppt.LiedPresentator.ILiedFormatter>();
        }
    }
}
