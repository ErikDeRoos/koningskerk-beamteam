using ISlideBuilder;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PowerpointGenerater.AppFlow
{
    class DefaultContainer : UnityContainer
    {
        public void RegisterAll()
        {
            this.RegisterInstance<IUnityContainer>(this);  // de main unity container
            this.RegisterType<MainForm, Form1>();  // opstart scherm
            this.RegisterType<SettingsForm, Instellingenform>();  // settings scherm
            RegisterType<IBuilder>("MicrosoftPowerpointWrapper.dll");  // powerpoint functionaliteit
        }

        private void RegisterType<T>(string takeFirstTypeFromThisAssembly)
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
