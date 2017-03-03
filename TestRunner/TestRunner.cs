using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TestRunner.Interfaces;

namespace TestRunner
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class TestRunner : StatefulService, ITestRunner
    {
        public TestRunner(StatefulServiceContext context)
            : base(context)
        {
            
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener(context =>
            {
                communicationListener = new CommunicationListener<TestRunner>(this);
                return new CompositeCommunicationListener(communicationListener, this.CreateServiceRemotingListener(context));
            }) };
        }

        public Task<string[]> Tests()
        {
            return communicationListener.Tests();
        }

        public Task<Result> Run(string testName)
        {
            return communicationListener.Run(testName);
        }

        CommunicationListener<TestRunner> communicationListener;        
    }
}
