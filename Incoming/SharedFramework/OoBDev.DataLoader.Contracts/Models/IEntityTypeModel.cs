using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Runtime.CompilerServices;

namespace OoBDev.DataLoader.Models
{
    public interface IEntityTypeModel
    {
        DbContext DbContext { get; }
        ITuple DefaultPrimaryKey { get; }
        ValidEntityModel[] Entities { get; }

#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        IReadOnlyEntityType EntityType { get; }
#else
        // IEntityType
        IEntityType EntityType { get; }
#endif
    }
}