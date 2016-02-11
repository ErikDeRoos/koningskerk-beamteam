using IDatabase;
using ILiturgieDatabase;
using ISettings;
using ISlideBuilder;
using Microsoft.Practices.Unity;
using PowerpointGenerater.Database;
using PowerpointGenerater.Settings;
using PowerpointGenerator.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PowerpointGenerater
{
    class DefaultContainer : UnityContainer
    {
        public void RegisterAll()
        {
            this.RegisterInstance<IUnityContainer>(this);  // de main unity container
            RegisterTypeWhenResolvableInAssembly<IBuilder>("MicrosoftPowerpointWrapper.dll");  // powerpoint functionaliteit
            this.RegisterInstance<IInstellingenFactory>(new SettingsFactory(System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "instellingen.xml", "masks.xml"));
            this.RegisterType(typeof(IEngine<>), typeof(FileEngine<>));
            this.RegisterType<ILiturgieLosOp, LiturgieDatabase>();
        }

        private void RegisterTypeWhenResolvableInAssembly<T>(string takeFirstTypeFromThisAssembly)
        {
            Type resolveBuilderType = null;
            try
            {
                var assembly = Assembly.LoadFrom(takeFirstTypeFromThisAssembly);
                resolveBuilderType = assembly.GetLoadableTypesWithInterface<T>().FirstOrDefault();
            }
            catch { }
            if (resolveBuilderType != null)
                this.RegisterType(typeof(T), resolveBuilderType);
        }
    }

    static class TypeLoaderExtensions
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
        public static IEnumerable<Type> GetLoadableTypes<T>(this Assembly assembly, bool excludeSystemTypes = true)
        {
            var it = typeof(T);
            var allTypes = GetLoadableTypes(assembly).Where(it.IsAssignableFrom);
            if (excludeSystemTypes)
                return allTypes.Where(t => !t.FullName.StartsWith("System."));
            return allTypes;
        }
        public static IEnumerable<Type> GetLoadableTypesWithInterface<T>(this Assembly assembly, bool excludeSystemTypes = true)
        {
            var qualifiedInterfaceName = typeof(T).FullName;
            var interfaceFilter = new TypeFilter(InterfaceFilter);
            return GetLoadableTypes<T>(assembly, excludeSystemTypes).Where(t => t.FindInterfaces(interfaceFilter, qualifiedInterfaceName).Any());
        }
        private static bool InterfaceFilter(Type typeObj, Object criteriaObj)
        {
            return typeObj.ToString() == criteriaObj.ToString();
        }
    }
}
