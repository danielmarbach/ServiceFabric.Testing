using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;

namespace TestRunner.NUnit
{
    /// <summary>
    /// The communication listener reflects the assembly containing TService for tests and loads them into an
    /// NUnitTestAssemblyRunner.
    /// It also acts as a gateway to the hosted tests. The listener is stateful and there is a coupling between the Run method
    /// and the Tests method.
    /// Only test names that are returned from the Tests method can be passed into the Run method.
    /// </summary>
    class CommunicationListener<TService> : ICommunicationListener
        where TService : StatefulService
    {
        public CommunicationListener(TService statefulService)
        {
            this.statefulService = statefulService;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
                var settings = new Dictionary<string, object>
                {
                    {"SynchronousEvents", true} // crucial to run listeners sync
                };
                var testSuite = runner.Load(typeof(TService).Assembly, settings);
                var testNameCache = new HashSet<string>();
                CacheTests(testNameCache, testSuite);
                cachedTestNames = Task.FromResult(testNameCache.ToArray());

                return "";
            });
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => runner.StopRun(true));
        }

        public void Abort()
        {
            // fire & forget
            CloseAsync(CancellationToken.None);
        }

        public Task<string[]> Tests()
        {
            return cachedTestNames;
        }


        public Task<Result> Run(string testName)
        {
            return Task.Run(() =>
            {
                var resultListener = new ResultListener();
                var provider = new StatefulServiceProviderListener<TService>(statefulService);
                var eventSourceTestListener = new EventSourceTestListener();
                var compositeListener = new CompositeListener(provider, resultListener, eventSourceTestListener);

                var fullNameFilter = new FullNameFilter(testName);
                runner.Run(compositeListener, fullNameFilter);

                return resultListener.Result;
            });
        }

        static void CacheTests(HashSet<string> testNameCache, ITest test)
        {
            var testIsSuite = test.IsSuite;
            if (testIsSuite)
            {
                foreach (var child in test.Tests)
                {
                    CacheTests(testNameCache, child);
                }
            }

            if (testIsSuite || test.RunState != RunState.Runnable)
            {
                return;
            }

            testNameCache.Add(test.FullName);
        }

        NUnitTestAssemblyRunner runner;

        Task<string[]> cachedTestNames;
        TService statefulService;
    }
}