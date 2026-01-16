using System;
using System.IO;

namespace OoBDev.DocumentCenter.Storage
{
    public class DocumentKeyGenerator : IDocumentKeyGenerator
    {
        public string Generate(string baseFileName, DateTimeOffset timestamp)
        {
            return $@"{Path.GetFileNameWithoutExtension(baseFileName)}_{timestamp.Ticks}{Path.GetExtension(baseFileName)}";
        }
    }
}
