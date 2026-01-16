using System;

namespace OoBDev.DocumentCenter.Contracts.Handlers
{
    public class UnhandledConversionRequestedException : NotSupportedException
    {
        public UnhandledConversionRequestedException(
            DocumentTypes inputType,
            DocumentTypes outputType
            ): base($"No chain found from \"{inputType}\" to \"{outputType}\"")
        {
            InputType = inputType;
            OutputType = outputType;
        }

        public DocumentTypes InputType { get; }
        public DocumentTypes OutputType { get; }
    }
}
