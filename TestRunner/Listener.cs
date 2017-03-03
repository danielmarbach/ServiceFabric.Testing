using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace TestRunner
{
    class Listener : ITestListener
    {
        public void TestStarted(ITest test)
        {
        }

        public void TestFinished(ITestResult result)
        {
            if (!result.Test.IsSuite)
            {
                switch (result.ResultState.Status)
                {
                    case TestStatus.Passed:
                        break;
                    case TestStatus.Inconclusive:
                        Exception = new InconclusiveException(result.Message);
                        break;
                    case TestStatus.Skipped:
                        Exception = new IgnoreException(result.Message);
                        break;
                    case TestStatus.Warning:
                        break;
                    case TestStatus.Failed:
                        Exception = new AssertionException($"{result.Message}{Environment.NewLine}{result.StackTrace}");
                        break;
                }

                Output = result.Output;
            }
        }

        public void TestOutput(TestOutput output)
        {
        }

        public Exception Exception { get; private set; }
        public string Output { get; private set; }
    }
}