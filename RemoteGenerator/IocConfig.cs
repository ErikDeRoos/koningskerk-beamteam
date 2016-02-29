using Microsoft.Practices.Unity;

namespace RemoteGenerator
{
    static class IocConfig
    {
        internal static void SetDefault(IUnityContainer container)
        {
            container.RegisterType<ISlideBuilder.IBuilder, mppt.PowerpointFunctions>();
            container.RegisterType<IFileSystem.IFileOperations, Tools.LocalFileOperations>();
            container.RegisterType<RemoteGenerator.Builder.IPpGenerator, RemoteGenerator.Builder.PpGenerator>(new ContainerControlledLifetimeManager());
        }
    }
}
