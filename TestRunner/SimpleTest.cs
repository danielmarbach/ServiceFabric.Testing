using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using NUnit.Framework;

namespace TestRunner
{
    [TestFixture]
    public class TestAccessingReliableStateManager : INeed<IReliableStateManager>
    {
        private IReliableStateManager stateManager;

        [Test]
        public async Task SomeTest()
        {
            var state = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>("state").ConfigureAwait(false);
            Assert.AreEqual("somestate", state.Name);
        }

        [TearDown]
        public void TearDown()
        {

        }

        public void Need(IReliableStateManager dependency)
        {
            stateManager = dependency;
        }

    }

    [TestFixture]
    public class TestAccessingStatefulContext : INeed<StatefulServiceContext>
    {
        private StatefulServiceContext statefulServiceContext;

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void SomeTest()
        {
            Console.WriteLine(statefulServiceContext.ServiceName);
            Console.WriteLine(statefulServiceContext.ReplicaId);
            Assert.AreEqual("TestRunnerType", statefulServiceContext.ServiceTypeName);
        }

        public static IEnumerable<object> Input => Enumerable.Range(0, 100).Cast<object>();

        [TearDown]
        public void TearDown()
        {

        }


        public void Need(StatefulServiceContext dependency)
        {
            statefulServiceContext = dependency;
        }

    }

    [TestFixture]
    public class TheoryTest
    {
        [SetUp]
        public void SetUp()
        {
        }

        [TestCase()]
        [TestCaseSource("Input")]
        public void SomeTest(int input)
        {
            Console.WriteLine(input);
            Assert.AreEqual(0, input % 3);
        }

        public static IEnumerable<object> Input => Enumerable.Range(0, 20).Cast<object>();

        [TearDown]
        public void TearDown()
        {

        }
    }

    //[TestFixture]
    //public class TestCode
    //{

    //    [Test]
    //    [Explicit]
    //    public async Task Test()
    //    {
    //        var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
    //        var testSuite = runner.Load(this.GetType().Assembly, new Dictionary<string, object> { { "SynchronousEvents", true } });
    //        FillTests(testSuite);

    //        foreach (var test in testNamesToTest)
    //        {
    //            var resultListener = new Listener();
    //            runner.Run(new ContextAwareTestListener(null), new FullNameFilter(test.Key));

    //            if (!string.IsNullOrEmpty(resultListener.Output))
    //            {
    //                Console.Write(resultListener.Output);
    //            }

    //            if (resultListener.Exception != null)
    //            {
    //                throw resultListener.Exception;
    //            }
    //        }
    //    }

    //    void FillTests(ITest test)
    //    {
    //        var testIsSuite = test.IsSuite;
    //        if (testIsSuite)
    //        {
    //            foreach (var child in test.Tests)
    //            {
    //                FillTests(child);
    //            }
    //        }

    //        if (testIsSuite || test.RunState != RunState.Runnable)
    //        {
    //            return;
    //        }

    //        if (!testNamesToTest.ContainsKey(test.FullName))
    //        {
    //            testNamesToTest.Add(test.FullName, test);
    //        }
    //    }


    //    Dictionary<string, ITest> testNamesToTest = new Dictionary<string, ITest>();

    //    class ContextAwareTestListener : ITestListener
    //    {
    //        private StatefulService statefulService;

    //        public ContextAwareTestListener(StatefulService service)
    //        {
    //            statefulService = service;
    //        }

    //        public void TestStarted(ITest test)
    //        {
    //            test.Properties.Add("StatefulService", statefulService);
    //        }

    //        public void TestFinished(ITestResult result)
    //        {
    //        }

    //        public void TestOutput(TestOutput output)
    //        {
    //        }
    //    }

    //    class Listener : ITestListener
    //    {
    //        public void TestStarted(ITest test)
    //        {
    //        }

    //        public void TestFinished(ITestResult result)
    //        {
    //            if (!result.Test.IsSuite)
    //            {
    //                switch (result.ResultState.Status)
    //                {
    //                    case TestStatus.Passed:
    //                        break;
    //                    case TestStatus.Inconclusive:
    //                        Exception = new InconclusiveException(result.Message);
    //                        break;
    //                    case TestStatus.Skipped:
    //                        Exception = new IgnoreException(result.Message);
    //                        break;
    //                    case TestStatus.Warning:
    //                        break;
    //                    case TestStatus.Failed:
    //                        Exception = new AssertionException($"{result.Message}{Environment.NewLine}{result.StackTrace}");
    //                        break;
    //                }

    //                Output = result.Output;
    //            }
    //        }

    //        public void TestOutput(TestOutput output)
    //        {
    //        }

    //        public Exception Exception { get; private set; }
    //        public string Output { get; private set; }
    //    }
    //}
}