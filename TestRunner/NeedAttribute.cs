using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.ServiceFabric.Services.Runtime;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace TestRunner
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class NeedAttribute : Attribute, ITestAction
    {
        public void BeforeTest(ITest test)
        {
            if (test.IsSuite && test.Fixture != null)
            {
                var testFixtureType = test.Fixture.GetType();

                var typeInfo = typeInfoCache.GetOrAdd(testFixtureType, fixtureType =>
                {
                    return (from t in fixtureType.GetInterfaces()
                            where t.IsGenericType
                            let dependencyType = t.GetGenericArguments()[0]
                            let genericType = typeof(INeed<>).MakeGenericType(dependencyType)
                            where genericType.IsAssignableFrom(t)
                            select new TypeInfo {GenericType = genericType, DependencyType = dependencyType})
                        .Distinct()
                        .ToArray();
                });

                var statefulService = (StatefulService) test.Properties.Get("StatefulService");
                var properties = statefulServicePropertyCache.GetOrAdd(statefulService.GetType(), type =>
                {
                    return
                        type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                            .ToDictionary(p => p.PropertyType, p => p);
                });

                foreach (var type in typeInfo)
                {
                    PropertyInfo property;
                    if (properties.TryGetValue(type.DependencyType, out property))
                    {
                        var methodInfo = testFixtureType.GetInterfaceMap(type.GenericType).TargetMethods.FirstOrDefault();
                        if (methodInfo != null)
                        {
                            methodInfo.Invoke(test.Fixture, new[] { property.GetValue(statefulService) });
                        }
                    }
                }
            }
        }

        public void AfterTest(ITest test)
        {
        }

        public ActionTargets Targets
        {
            get {  return ActionTargets.Suite; }
        }

        struct TypeInfo
        {
            public Type GenericType;
            public Type DependencyType;
        }

        static ConcurrentDictionary<Type, TypeInfo[]> typeInfoCache = new ConcurrentDictionary<Type, TypeInfo[]>();
        static ConcurrentDictionary<Type, Dictionary<Type, PropertyInfo>> statefulServicePropertyCache = new ConcurrentDictionary<Type, Dictionary<Type, PropertyInfo>>();
    }
}