using System;

namespace OoBDev.DocumentCenter.Abstractions.Storage
{
    public interface IBlobContentInfoResult
    {
        string? ContentType { get; }
        string? FileName { get; }
        long? FileSize { get; }
        DateTimeOffset? LastModified { get; }
    }
}