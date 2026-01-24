using System;

namespace OoBDev.ToolKit.IO;

public interface ITempFile : IDisposable
{
    string FilePath { get; }
}
