using System;
using System.Runtime.Serialization;

namespace OoBDev.Accounting.Contracts
{
    [Serializable]
    public class UnknownAccountingException : Exception
    {
        public UnknownAccountingException()
        {
        }

        public UnknownAccountingException(string message) : base(message)
        {
        }

        public UnknownAccountingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnknownAccountingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
