using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using NUnit.Framework;

namespace TestRunner
{
    public abstract class StatefulServiceContextAwareBase
    {
        public IReliableStateManager StateManager { get; set; } =
            TestContext.CurrentContext.Test.Properties.Get("ReliableStateManager") as IReliableStateManager;
    }

    [TestFixture]
    public class SimpleTest : StatefulServiceContextAwareBase
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public async Task SomeTest()
        {
            var state = await StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("state").ConfigureAwait(false);
            Assert.Fail();
        }

        [TearDown]
        public void TearDown()
        {
            
        }

    }

    [TestFixture]
    public class AnotherTest : StatefulServiceContextAwareBase
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public async Task SomeTest()
        {
            var state = await StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("state").ConfigureAwait(false);
            await Task.Delay(1000);
            Assert.True(true);
        }

        [TearDown]
        public void TearDown()
        {

        }

    }

    [TestFixture]
    public class RepeatedTest : StatefulServiceContextAwareBase
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        [Repeat(10)]
        public async Task SomeTest()
        {
            var state = await StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("state").ConfigureAwait(false);
            await Task.Delay(100);
            Assert.True(true);
        }

        [TearDown]
        public void TearDown()
        {

        }

    }

    [TestFixture]
    public class TheoryTest : StatefulServiceContextAwareBase
    {
        [SetUp]
        public void SetUp()
        {
        }

        [TestCase()]
        [TestCaseSource("Input")]
        public void SomeTest(int input)
        {
            Assert.AreEqual(0, input % 3);
        }

        public static IEnumerable<object> Input => Enumerable.Range(0, 10000).Cast<object>();

        [TearDown]
        public void TearDown()
        {

        }

    }
}