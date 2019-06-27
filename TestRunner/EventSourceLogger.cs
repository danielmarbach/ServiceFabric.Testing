using System.Diagnostics.Tracing;
using System.Threading.Tasks;

namespace TestRunner.NUnit
{
    [EventSource(Name = "TestRunner")]
    sealed class EventSourceLogger : EventSource
    {
        static readonly EventSourceLogger logger = new EventSourceLogger();

        static EventSourceLogger()
        {
            // A workaround for the problem where ETW activities do not get tracked until Tasks infrastructure is initialized.
            // This problem will be fixed in .NET Framework 4.6.2.
            Task.Run(() => { });
        }

        EventSourceLogger() { }

        public static EventSourceLogger GetLogger()
        {
            return logger;
        }

        public static class Keywords
        {
            public const EventKeywords LogMessageRequest = (EventKeywords)0x1L;
        }

        [Event(eventId: 1, Keywords = Keywords.LogMessageRequest, Level = EventLevel.Informational, Message = "{0}")]
        public void Information(string message)
        {
            WriteEvent(1, message);
        }
    }
}