using Autofac;
using RemoteGenerator.Properties;

namespace RemoteGenerator
{
    static class Bootstrap
    {
        internal static void SetDefault(ContainerBuilder container)
        {
            container.RegisterType<mppt.PowerpointFunctions>().As<ISlideBuilder.IBuilder>();
            container.RegisterType<Tools.LocalFileOperations>().As<IFileSystem.IFileOperations>();
            container.RegisterType<Builder.PpGenerator>().As<Builder.IPpGenerator>().SingleInstance();
            container.RegisterType<WCF.Host>().As<WCF.IHost>().WithParameter("address", Settings.Default.BindOnAddress);
            container.RegisterType<Form1>().As<System.Windows.Forms.Form>();
        }
    }
}
