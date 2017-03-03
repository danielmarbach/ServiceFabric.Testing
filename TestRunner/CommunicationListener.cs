using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Filters;
using TestRunner.Interfaces;

namespace TestRunner
{
    class CommunicationListener<TService> : ICommunicationListener
        where TService : StatefulService
    {
        private NUnitTestAssemblyRunner runner;

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
                var testSuite = runner.Load(GetType().Assembly, settings);
                HashSet<string> testNameCache = new HashSet<string>();
                CacheTests(testNameCache, testSuite);
                cachedTestNames = Task.FromResult(testNameCache.ToArray());

                return "";
            });
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public void Abort()
        {
        }

        public Task<string[]> Tests()
        {
            return cachedTestNames;
        }

        public Task<Result> Run(string testName)
        {
            return Task.Run(() =>
            {
                var resultListener = new Listener();
                var provider = new StatefulServiceProviderListener<TService>(statefulService);
                var compositeListener = new CompositeListener(provider, resultListener);

                var fullNameFilter = new FullNameFilter(testName);
                runner.Run(compositeListener, fullNameFilter);

                var result = new Result(resultListener.Output, resultListener.Exception);
                return result;
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

        Task<string[]> cachedTestNames;
        TService statefulService;
    }
}