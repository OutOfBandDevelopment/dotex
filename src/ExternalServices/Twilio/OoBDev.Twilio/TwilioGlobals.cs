namespace OoBDev.Twilio;

/// <summary>Provider keys of the Twilio adapter.</summary>
public static class TwilioGlobals
{
    /// <summary>The kebab-case provider key channels refer to.</summary>
    public const string ProviderName = "twilio";

    /// <summary>The protocol the provider handles.</summary>
    public const string SmsProtocol = "sms";

    /// <summary>The default configuration section.</summary>
    public const string ConfigurationSection = "OoBDev:Twilio";
}
