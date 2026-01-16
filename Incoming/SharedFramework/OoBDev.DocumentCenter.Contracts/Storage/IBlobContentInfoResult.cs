using System;

namespace OoBDev.DocumentCenter.Contracts.Storage
{
    public interface IBlobContentInfoResult
    {
        string? ContentType { get; }
        string? FileName { get; }
        long? FileSize { get; }
        DateTimeOffset? LastModified { get; }
    }
}