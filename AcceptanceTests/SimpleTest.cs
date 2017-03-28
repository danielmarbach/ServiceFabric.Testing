using System;
using NUnit.Framework;

namespace AcceptanceTests
{
    [TestFixture]
    public class SimpleTest
    {
        [Test]
        public void WithConsole()
        {
            Console.WriteLine("Foo");
            Console.WriteLine("Bar");
        }

        [Test]
        public void Ignored()
        {
            Assert.Ignore("Ignored");
        }

        [Test]
        public void Inconclusive()
        {
            Assert.Inconclusive("Inconclusive");
        }

        [Test]
        public void Failing()
        {
            Assert.True(false);
        }

        [Test]
        public void WillSeeEnvironmentVariablesPromoted()
        {
            Console.WriteLine(Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString", EnvironmentVariableTarget.Process).Substring(0, 15));
            Console.WriteLine(Environment.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString", EnvironmentVariableTarget.Process).Substring(0, 15));
            Console.WriteLine(Environment.GetEnvironmentVariable("Transport.UseSpecific", EnvironmentVariableTarget.Process));
        }
    }
}