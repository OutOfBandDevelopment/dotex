using System;
using System.Runtime.Serialization;

namespace OoBDev.Accounting.Contracts
{
    [Serializable]
    public class AccountingValidationException : Exception
    {
        public AccountingValidationException()
        {
        }

        public AccountingValidationException(string message) : base(message)
        {
        }

        public AccountingValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AccountingValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
