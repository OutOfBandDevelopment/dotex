using System;

namespace OoBDev.DocumentCenter.Contracts.Handlers
{
    public class InvalidConversionInputException : NotSupportedException
    {
        public InvalidConversionInputException(
            DocumentTypes inputType
            ) : base($"Invalid input type \"{inputType}\" requested")
        {
            InputType = inputType;
        }

        public DocumentTypes InputType { get; }
    }
}
