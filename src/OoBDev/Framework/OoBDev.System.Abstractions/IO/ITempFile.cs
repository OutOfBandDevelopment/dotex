using System;

namespace OoBDev.System.IO;

public interface ITempFile : IDisposable
{
    string FilePath { get; }
}
