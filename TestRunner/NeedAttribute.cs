using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.ServiceFabric.Services.Runtime;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace TestRunner.NUnit
{
    /// <summary>
    /// This attribute is an extensibility point of type ITestAction that is applied to the <see cref="INeed{TDependency}" />
    /// interface. The implementation does the following:
    /// - When the test fixture implements <see cref="INeed{TDependency}" /> the dependency type is search on the
    /// StatefulService hierarchy.
    /// - If a property of type TDependency exists the instance will be passed into the Need method.
    /// - Caching is applied for performance reasons.
    /// </summary>
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
                            select new TypeInfo
                            {
                                GenericType = genericType,
                                DependencyType = dependencyType
                            })
                        .Distinct()
                        .ToArray();
                });

                var statefulService = (StatefulService) test.Properties.Get("StatefulService");
                var properties = statefulServicePropertyCache.GetOrAdd(statefulService.GetType(), type =>
                {
                    return
                        type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                            .Distinct(Comparer)
                            .ToDictionary(p => p.PropertyType, p => p);
                });

                foreach (var type in typeInfo)
                {
                    if (properties.TryGetValue(type.DependencyType, out var property))
                    {
                        var methodInfo = testFixtureType.GetInterfaceMap(type.GenericType).TargetMethods.FirstOrDefault();
                        if (methodInfo != null)
                        {
                            methodInfo.Invoke(test.Fixture, new[]
                            {
                                property.GetValue(statefulService)
                            });
                        }
                    }
                }
            }
        }

        public void AfterTest(ITest test)
        {
        }

        public ActionTargets Targets => ActionTargets.Suite;

        static ConcurrentDictionary<Type, TypeInfo[]> typeInfoCache = new ConcurrentDictionary<Type, TypeInfo[]>();
        static ConcurrentDictionary<Type, Dictionary<Type, PropertyInfo>> statefulServicePropertyCache = new ConcurrentDictionary<Type, Dictionary<Type, PropertyInfo>>();

        static PropertyTypeComparer Comparer = new PropertyTypeComparer();

        struct TypeInfo
        {
            public Type GenericType;
            public Type DependencyType;
        }

        class PropertyTypeComparer : IEqualityComparer<PropertyInfo>
        {
            public bool Equals(PropertyInfo x, PropertyInfo y)
            {
                return x.PropertyType == y.PropertyType;
            }

            public int GetHashCode(PropertyInfo obj)
            {
                return obj.PropertyType.GetHashCode();
            }
        }
    }
}