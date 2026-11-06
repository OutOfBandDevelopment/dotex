namespace OoBDev.MailKit;

/// <summary>Provider keys of the MailKit adapter.</summary>
public static class MailKitGlobals
{
    /// <summary>The kebab-case provider key channels refer to.</summary>
    public const string ProviderName = "mailkit";

    /// <summary>The kebab-case provider key of the IMAP inbox poller.</summary>
    public const string ImapProviderName = "mailkit-imap";

    /// <summary>The protocol the provider handles.</summary>
    public const string EmailProtocol = "email";
}
