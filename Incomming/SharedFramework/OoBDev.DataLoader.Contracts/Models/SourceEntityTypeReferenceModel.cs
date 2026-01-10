using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OoBDev.DataLoader.Models
{
    [DebuggerDisplay("{EntityType}: {HasDuplicates}")]
    public class SourceEntityTypeReferenceModel
    {
#if NET5_0_OR_GREATER
        public IReadOnlyEntityType EntityType { get; internal set; }
#else
        public IEntityType EntityType { get; internal set; }
#endif
        public IEnumerable<IGrouping<ITuple, SourceEntityReferenceModel>> Entities { get; internal set; }
        public bool HasDuplicates { get; internal set; }
        public DbContext DbContext { get; internal set; }
        public ITuple DefaultPrimaryKey { get; internal set; }
    }
}
