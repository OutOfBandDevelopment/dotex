namespace OoBDev.Communications.Abstractions;

/// <summary>How urgent a request is; quiet hours defer only <see cref="Normal"/>.</summary>
public enum RequestPriority
{
    /// <summary>Informational or promotional; honors quiet hours.</summary>
    Normal = 0,

    /// <summary>Important, such as a password reset; ignores quiet hours.</summary>
    High = 1,

    /// <summary>Security or account alerts; ignores quiet hours.</summary>
    Critical = 2,
}
