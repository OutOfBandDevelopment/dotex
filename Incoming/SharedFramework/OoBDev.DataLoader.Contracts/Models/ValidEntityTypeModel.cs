using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace OoBDev.DataLoader.Models
{
    [DebuggerDisplay("{EntityType}")]
    public class ValidEntityTypeModel : IEntityTypeModel
    {
#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        public IReadOnlyEntityType EntityType { get; internal set; }
#else
        // IEntityType
        public IEntityType EntityType { get; internal set; }
#endif
        public DbContext DbContext { get; internal set; }
        public ITuple DefaultPrimaryKey { get; internal set; }
        public ValidEntityModel[] Entities { get; internal set; }
    }
}