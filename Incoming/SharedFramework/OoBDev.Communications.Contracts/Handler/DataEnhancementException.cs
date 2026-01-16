using System;

namespace OoBDev.Communications.Contracts.Handler
{
    public class DataEnhancementException : Exception
    {
        public DataEnhancementException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}