using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace TestRunner.NUnit
{
    public abstract class AbstractTestRunner<TSelf> : StatefulService, ITestRunner
        where TSelf : StatefulService
    {
        protected AbstractTestRunner(StatefulServiceContext context)
            : base(context)
        {
        }

        protected abstract TSelf Self { get; }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener(context =>
            {
                communicationListener = new CommunicationListener<TSelf>(Self);
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

        CommunicationListener<TSelf> communicationListener;
    }
}