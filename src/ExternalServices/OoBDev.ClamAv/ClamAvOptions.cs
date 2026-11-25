using System.ComponentModel.DataAnnotations;

namespace OoBDev.ClamAv;

/// <summary>Options of the ClamAV scanner (section <c>ClamAv</c>).</summary>
public sealed class ClamAvOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "ClamAv";

    /// <summary>The clamd host.</summary>
    [Required]
    public string Host { get; set; } = "localhost";

    /// <summary>The clamd TCP port.</summary>
    [Range(1, 65535)]
    public int Port { get; set; } = 3310;

    /// <summary>Seconds allowed for connecting, sending and waiting for the verdict.</summary>
    [Range(1, 3600)]
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>Bytes per INSTREAM chunk; clamd refuses a stream larger than its <c>StreamMaxLength</c>.</summary>
    [Range(1024, 1_048_576)]
    public int ChunkSize { get; set; } = 65_536;
}
