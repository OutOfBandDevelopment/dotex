using System;

namespace OoBDev.DocumentCenter.Storage
{
    public interface IDocumentKeyGenerator
    {
        string Generate(string baseFileName, DateTimeOffset timestamp);
    }
}
