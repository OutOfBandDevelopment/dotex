using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace OoBDev.DataLoader.Models
{
    [DebuggerDisplay("{KeyValue}: {Source}")]
    public class SourceEntityReferenceModel
    {
        public SourceDataReferenceModel Source { get; internal set; }
        public object[] KeyData { get; internal set; }
        public ITuple? KeyValue { get; internal set; }
        public object? Entity { get; internal set; }
    }
}
