using System;

namespace OoBDev.DocumentCenter.Abstractions.Handlers
{
    public class InvalidConversionOutputException : NotSupportedException
    {
        public InvalidConversionOutputException(
            DocumentTypes outputType
            ) : base($"Invalid output type \"{outputType}\" requested")
        {
            OutputType = outputType;
        }

        public DocumentTypes OutputType { get; }
    }
}
