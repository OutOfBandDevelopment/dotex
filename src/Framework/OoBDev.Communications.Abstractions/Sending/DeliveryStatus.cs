namespace OoBDev.Communications.Abstractions;

/// <summary>The delivery state of a message; the order is the order of valid transitions.</summary>
public enum DeliveryStatus
{
    /// <summary>Accepted for sending.</summary>
    Queued,

    /// <summary>Handed to the vendor.</summary>
    Sent,

    /// <summary>Delivered to the recipient.</summary>
    Delivered,

    /// <summary>The send failed.</summary>
    Failed,

    /// <summary>The recipient address bounced.</summary>
    Bounced,

    /// <summary>The recipient opened the message.</summary>
    Opened,

    /// <summary>The recipient followed a link.</summary>
    Clicked,

    /// <summary>The recipient marked the message as spam. Last in the order so it still applies after an open or click.</summary>
    Complained,
}
