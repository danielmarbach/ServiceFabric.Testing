using NUnit.Framework.Interfaces;

namespace TestRunner.NUnit
{
    /// <summary>
    /// Orchestrates multiple test listeners
    /// </summary>
    class CompositeListener : ITestListener
    {
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

        public void SendMessage(TestMessage message)
        {
            foreach(var testListener in testListeners)
            {
                testListener.SendMessage(message);
            }
        }

        ITestListener[] testListeners;
    }
}