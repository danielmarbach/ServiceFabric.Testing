using Microsoft.ServiceFabric.Services.Runtime;
using NUnit.Framework.Interfaces;

namespace TestRunner.NUnit
{
    /// <summary>
    /// Provides the service into the TestContext.
    /// </summary>
    /// <typeparam name="TService">The service.</typeparam>
    class StatefulServiceProviderListener<TService> : ITestListener
        where TService : StatefulService
    {
        public StatefulServiceProviderListener(TService service)
        {
            this.service = service;
        }

        public void TestStarted(ITest test)
        {
            test.Properties.Set("StatefulService", service);
        }

        public void TestFinished(ITestResult result)
        {
        }

        public void TestOutput(TestOutput output)
        {
        }

        public void SendMessage(TestMessage message)
        {
        }

        TService service;
    }
}