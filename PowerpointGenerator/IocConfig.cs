using Microsoft.Practices.Unity;

namespace PowerpointGenerator
{
    static class IocConfig
    {
        internal static void SetDefault(IUnityContainer container)
        {
            container.RegisterType<ILiturgieDatabase.ILiturgieLosOp, PowerpointGenerator.Database.LiturgieDatabase>();
            container.RegisterType(typeof(IDatabase.IEngine<>), typeof(PowerpointGenerator.Database.FileEngine<>));
            container.RegisterType<ISlideBuilder.IBuilder, mppt.PowerpointFunctions>();
            container.RegisterType<IFileSystem.IFileOperations, Tools.LocalFileOperations>();
        }
    }
}
