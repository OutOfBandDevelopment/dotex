using System.Diagnostics;

namespace OoBDev.DataLoader.Models
{
    [DebuggerDisplay("{Reference}: {Data}")]
    public class SourceDataReferenceModel
    {
        public bool IsMasterData { get; internal set; }
        public object Data { get; internal set; }
        public string? Reference { get; internal set; }
    }
}
