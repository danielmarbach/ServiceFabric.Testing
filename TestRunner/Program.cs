using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;

namespace TestRunner
{
    internal static class Program
    {
        private static void Main()
        {
            ServiceRuntime.RegisterServiceAsync("TestRunnerType",
                context => new TestRunner(context)).GetAwaiter().GetResult();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
