using Microsoft.ServiceFabric.Services.Runtime;
using NUnit.Framework.Interfaces;

namespace TestRunner
{
    class StatefulServiceProviderListener<TService> : ITestListener
        where TService : StatefulService
    {
        private TService service;

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
    }
}