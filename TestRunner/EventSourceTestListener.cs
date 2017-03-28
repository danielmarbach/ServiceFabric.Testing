using NUnit.Framework.Interfaces;

namespace TestRunner
{
    public class EventSourceTestListener : ITestListener
    {
        EventSourceLogger logger = EventSourceLogger.GetLogger();

        public void TestStarted(ITest test)
        {
        }

        public void TestFinished(ITestResult result)
        {
            if (!result.Test.IsSuite)
            {
                var message = $"Finished {result.FullName}. Status: {result.ResultState.Status}. Message: {result.Output}";
                logger.Information(message);
            }
        }

        public void TestOutput(TestOutput output)
        {
        }
    }
}