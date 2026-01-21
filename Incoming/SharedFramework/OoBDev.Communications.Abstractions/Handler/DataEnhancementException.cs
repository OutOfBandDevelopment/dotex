using System;

namespace OoBDev.Communications.Abstractions.Handler
{
    public class DataEnhancementException : Exception
    {
        public DataEnhancementException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}