using System;
using System.Runtime.Serialization;
using NUnit.Framework;

namespace TestRunner.Interfaces
{
    [DataContract]
    [KnownType(typeof(AssertionException))]
    [KnownType(typeof(IgnoreException))]
    [KnownType(typeof(InconclusiveException))]
    public class Result
    {
        public Result(string output = null, Exception exception = null)
        {
            HasOutput = !string.IsNullOrEmpty(output);
            Output = output;
            Exception = exception;
            HasException = exception != null;
        }

        [DataMember]
        public bool HasOutput { get; set; }

        [DataMember]
        public string Output { get; set; }
        
        [DataMember]
        public Exception Exception { get; set; }

        [DataMember]
        public bool HasException { get; set; }
    }
}