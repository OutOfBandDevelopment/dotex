namespace OoBDev.SendGrid;

/// <summary>Provider keys of the SendGrid adapter.</summary>
public static class SendGridGlobals
{
    /// <summary>The kebab-case provider key channels refer to.</summary>
    public const string ProviderName = "sendgrid";

    /// <summary>The protocol the provider handles.</summary>
    public const string EmailProtocol = "email";

    /// <summary>The default configuration section.</summary>
    public const string ConfigurationSection = "OoBDev:SendGrid";
}
