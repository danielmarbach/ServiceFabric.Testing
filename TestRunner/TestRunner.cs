using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnitLite;

namespace TestRunner
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class TestRunner : StatefulService
    {
        public TestRunner(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener(context => this.CreateInternalListener(context)) };
        }

        private ICommunicationListener CreateInternalListener(ServiceContext context)
        {
            // Partition replica's URL is the node's IP, port, PartitionId, ReplicaId, Guid
            EndpointResourceDescription internalEndpoint = context.CodePackageActivationContext.GetEndpoint("Web");

            // Multiple replicas of this service may be hosted on the same machine,
            // so this address needs to be unique to the replica which is why we have partition ID + replica ID in the URL.
            // HttpListener can listen on multiple addresses on the same port as long as the URL prefix is unique.
            // The extra GUID is there for an advanced case where secondary replicas also listen for read-only requests.
            // When that's the case, we want to make sure that a new unique address is used when transitioning from primary to secondary
            // to force clients to re-resolve the address.
            // '+' is used as the address here so that the replica listens on all available hosts (IP, FQDM, localhost, etc.)

            string uriPrefix = String.Format(
                "{0}://+:{1}/{2}/{3}-{4}/",
                internalEndpoint.Protocol,
                internalEndpoint.Port,
                context.PartitionId,
                context.ReplicaOrInstanceId,
                Guid.NewGuid());

            string nodeIP = FabricRuntime.GetNodeContext().IPAddressOrFQDN;

            // The published URL is slightly different from the listening URL prefix.
            // The listening URL is given to HttpListener.
            // The published URL is the URL that is published to the Service Fabric Naming Service,
            // which is used for service discovery. Clients will ask for this address through that discovery service.
            // The address that clients get needs to have the actual IP or FQDN of the node in order to connect,
            // so we need to replace '+' with the node's IP or FQDN.
            string uriPublished = uriPrefix.Replace("+", nodeIP);
            return new HttpCommunicationListener(uriPrefix, uriPublished, this.ProcessInternalRequest);
        }

        private async Task ProcessInternalRequest(HttpListenerContext context, CancellationToken cancelRequest)
        {
            try
            {
                if (output.Count == 0)
                {
                    return;
                }

                using (HttpListenerResponse response = context.Response)
                using(var streamWriter = new StreamWriter(response.OutputStream))
                {
                    response.ContentType = "text/plain";
                    string line;
                    while (output.TryDequeue(out line))
                    {
                        await streamWriter.WriteLineAsync(line).ConfigureAwait(false);
                    }
                    await streamWriter.FlushAsync().ConfigureAwait(false);
                    streamWriter.Close();
                    response.Close();
                }
            }
            catch (Exception)
            {
                // stream closed etc.
            }
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
            runner.Load(GetType().Assembly, new Dictionary<string, object>());
            runner.RunAsync(new CompositeListener(
                new ContextAwareTestListener(StateManager),
                new TeamCityEventListener(new TextWriterConcurrenctQueueDecorator(output))), TestFilter.Empty);

            using (cancellationToken.Register(() => runner.StopRun(force: false)))
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }
        }

        ConcurrentQueue<string> output = new ConcurrentQueue<string>();

        class TextWriterConcurrenctQueueDecorator : StringWriter
        {
            private ConcurrentQueue<string> output;

            public TextWriterConcurrenctQueueDecorator(ConcurrentQueue<string> output)
            {
                this.output = output;
            }

            public override void WriteLine(string format, object arg0)
            {
                output.Enqueue(string.Format(format, arg0));
            }

            public override void WriteLine(string format, object arg0, object arg1)
            {
                output.Enqueue(string.Format(format, arg0, arg1));
            }

            public override void WriteLine(string format, object arg0, object arg1, object arg2)
            {
                output.Enqueue(string.Format(format, arg0, arg1, arg2));
            }

            public override void WriteLine(string format, params object[] arg)
            {
                output.Enqueue(string.Format(format, arg));
            }
        }

        class CompositeListener : ITestListener
        {
            private ITestListener[] testListeners;

            public CompositeListener(params ITestListener[] listeners)
            {
                testListeners = listeners;
            }

            public void TestStarted(ITest test)
            {
                foreach (var testListener in testListeners)
                {
                    testListener.TestStarted(test);
                }
            }

            public void TestFinished(ITestResult result)
            {
                foreach (var testListener in testListeners)
                {
                    testListener.TestFinished(result);
                }
            }

            public void TestOutput(TestOutput output)
            {
                foreach (var testListener in testListeners)
                {
                    testListener.TestOutput(output);
                }
            }
        }

        class ContextAwareTestListener : ITestListener
        {
            private IReliableStateManager statefulStateManager;

            public ContextAwareTestListener(IReliableStateManager stateManager)
            {
                statefulStateManager = stateManager;
            }

            public void TestStarted(ITest test)
            {
                test.Properties.Add("ReliableStateManager", statefulStateManager);
            }

            public void TestFinished(ITestResult result)
            {
            }

            public void TestOutput(TestOutput output)
            {
            }
        }

        public sealed class HttpCommunicationListener : ICommunicationListener
        {
            private readonly string publishUri;
            private readonly HttpListener httpListener;
            private readonly Func<HttpListenerContext, CancellationToken, Task> processRequest;
            private readonly CancellationTokenSource processRequestsCancellation = new CancellationTokenSource();

            public HttpCommunicationListener(string uriPrefix, string uriPublished, Func<HttpListenerContext, CancellationToken, Task> processRequest)
            {
                this.publishUri = uriPublished;
                this.processRequest = processRequest;
                this.httpListener = new HttpListener();
                this.httpListener.Prefixes.Add(uriPrefix);
            }

            public void Abort()
            {
                this.processRequestsCancellation.Cancel();
                this.httpListener.Abort();
            }

            public Task CloseAsync(CancellationToken cancellationToken)
            {
                this.processRequestsCancellation.Cancel();
                this.httpListener.Close();
                return Task.FromResult(true);
            }

            public Task<string> OpenAsync(CancellationToken cancellationToken)
            {
                this.httpListener.Start();

                Task openTask = this.ProcessRequestsAsync(this.processRequestsCancellation.Token);

                return Task.FromResult(this.publishUri);
            }

            // Only handles a request at a time on purpose
            private async Task ProcessRequestsAsync(CancellationToken processRequests)
            {
                while (!processRequests.IsCancellationRequested)
                {
                    HttpListenerContext request = await this.httpListener.GetContextAsync().ConfigureAwait(false);

                    await processRequest(request, this.processRequestsCancellation.Token).ConfigureAwait(false);
                }
            }
        }
    }
}
