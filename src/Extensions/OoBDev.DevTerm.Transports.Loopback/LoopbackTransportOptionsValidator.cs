using Microsoft.Extensions.Options;

namespace OoBDev.DevTerm.Transports.Loopback;

/// <summary>Validates <see cref="LoopbackTransportOptions"/>.</summary>
public sealed class LoopbackTransportOptionsValidator : IValidateOptions<LoopbackTransportOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, LoopbackTransportOptions options) =>
        options.SampleIntervalMs < 0
            ? ValidateOptionsResult.Fail("SampleIntervalMs must be 0 or greater.")
            : ValidateOptionsResult.Success;
}
