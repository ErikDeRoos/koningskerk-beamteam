// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.Practices.Unity.Configuration.ConfigurationHelpers;

namespace Unity.FactoryConfig
{
    public class FactoryElement : InjectionMemberElement
    {
        private const string typePropertyName = "type";
        private const string methodPropertyName = "method";
        private static int numFactories;
        private readonly int factoryNum;

        public FactoryElement()
        {
            factoryNum = Interlocked.Increment(ref numFactories);
        }

        [ConfigurationProperty(typePropertyName, IsKey = false, IsRequired = true)]
        public string TypeName
        {
            get { return (string)base[typePropertyName]; }
            set { base[typePropertyName] = value; }
        }

        [ConfigurationProperty(methodPropertyName, IsKey = false, IsRequired = true)]
        public string MethodName
        {
            get { return (string)base[methodPropertyName]; }
            set { base[methodPropertyName] = value; }
        }

        public override string Key
        {
            get { return "ConfiguredFactory " + factoryNum; }
        }

        public override IEnumerable<InjectionMember> GetInjectionMembers(IUnityContainer container, Type fromType,
            Type toType, string name)
        {
            return new InjectionMember[] { new InjectionFactory(GetFactoryFunc()) };
        }

        private Func<IUnityContainer, Type, string, object> GetFactoryFunc()
        {
            MethodInfo factoryMethod = FindFactoryMethod();

            if (IsZeroArgFactory(factoryMethod))
            {
                Delegate target = MakeTargetDelegate(typeof(Func<>), factoryMethod);
                return (c, t, name) => target.DynamicInvoke();
            }

            if (IsContainerOnlyArgFactory(factoryMethod))
            {
                Delegate target = MakeTargetDelegate(typeof(Func<,>), factoryMethod, typeof(IUnityContainer));
                return (c, t, name) => target.DynamicInvoke(c);
            }

            if (IsContainerTypeNameArgFactory(factoryMethod))
            {
                Delegate target = MakeTargetDelegate(typeof(Func<,,,>), factoryMethod,
                    typeof(IUnityContainer), typeof(Type), typeof(string));
                return (c, t, name) => target.DynamicInvoke(c, t, name);
            }

            return ThrowMethodNotValidFactory(factoryMethod);
        }

        private MethodInfo FindFactoryMethod()
        {
            Type factoryType = TypeResolver.ResolveType(TypeName);

            MethodInfo factoryMethod = factoryType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == MethodName).FirstOrDefault();

            if (factoryMethod == null)
            {
                ThrowFactoryNotFound(factoryType);
            }

            return factoryMethod;
        }

        private bool IsZeroArgFactory(MethodInfo factoryMethod)
        {
            return factoryMethod.GetParameters().Length == 0;
        }

        private bool IsContainerOnlyArgFactory(MethodInfo factoryMethod)
        {
            ParameterInfo[] methodParams = factoryMethod.GetParameters();
            return methodParams.Length == 1 &&
                methodParams[0].ParameterType == typeof(IUnityContainer);
        }

        private bool IsContainerTypeNameArgFactory(MethodInfo factoryMethod)
        {
            ParameterInfo[] methodParams = factoryMethod.GetParameters();
            return methodParams.Length == 3 &&
                methodParams[0].ParameterType == typeof(IUnityContainer) &&
                methodParams[1].ParameterType == typeof(Type) &&
                methodParams[2].ParameterType == typeof(string);

        }

        private Delegate MakeTargetDelegate(Type funcType, MethodInfo factoryMethod, params Type[] typeArguments)
        {
            Type targetDelegateType = funcType.MakeGenericType(typeArguments.Concat(new[] { factoryMethod.ReturnType }).ToArray());
            return Delegate.CreateDelegate(targetDelegateType, factoryMethod, true);
        }

        private Func<IUnityContainer, Type, string, object> ThrowMethodNotValidFactory(MethodInfo factoryMethod)
        {
            throw new InvalidOperationException(
                string.Format(CultureInfo.CurrentUICulture,
                    "The method {0}.{1} cannot be used as a factory", factoryMethod.ReflectedType.FullName, factoryMethod.Name));
        }

        private object ThrowFactoryNotFound(Type factoryType)
        {
            throw new InvalidOperationException(
                string.Format(CultureInfo.CurrentUICulture, "The factory method {0}.{1} was not found",
                    factoryType.FullName, MethodName));
        }
    }
}