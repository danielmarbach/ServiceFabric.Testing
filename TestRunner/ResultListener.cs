namespace TestRunner
{
    using System;
    using NUnit.Framework;
    using NUnit.Framework.Interfaces;

    class ResultListener : ITestListener
    {
        public Result Result { get; private set; }

        public void TestStarted(ITest test)
        {
        }

        public void TestFinished(ITestResult result)
        {
            if (!result.Test.IsSuite)
            {
                Exception exception = null;
                switch (result.ResultState.Status)
                {
                    case TestStatus.Passed:
                        break;
                    case TestStatus.Inconclusive:
                        exception = new InconclusiveException(result.Message);
                        break;
                    case TestStatus.Skipped:
                        exception = new IgnoreException(result.Message);
                        break;
                    case TestStatus.Warning:
                        break;
                    case TestStatus.Failed:
                        exception = new AssertionException($"{result.Message}{Environment.NewLine}{result.StackTrace}");
                        break;
                }

                Result = new Result(result.Output, exception)
                {
                    Duration = result.Duration,
                    StartTime = result.StartTime,
                    EndTime = result.EndTime
                };
            }
        }

        public void TestOutput(TestOutput output)
        {
        }
    }
}