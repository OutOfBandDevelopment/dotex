namespace OoBDev.Communications.Orchestration;

/// <summary>Names the template and data keys the template composers use.</summary>
public sealed class TemplateComposerOptions
{
    /// <summary>Gets or sets the channel name <see cref="TemplateEmailComposer"/> serves.</summary>
    public string EmailChannelName { get; set; } = "email";

    /// <summary>Gets or sets the channel name <see cref="TemplateSmsComposer"/> serves.</summary>
    public string SmsChannelName { get; set; } = "sms";

    /// <summary>Gets or sets the request data key that holds the email address.</summary>
    public string EmailAddressKey { get; set; } = "EmailAddress";

    /// <summary>Gets or sets the request data key that holds the phone number.</summary>
    public string PhoneNumberKey { get; set; } = "PhoneNumber";
}
