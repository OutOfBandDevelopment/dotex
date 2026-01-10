using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OoBDev.DataLoader
{
    [Serializable]
    internal class DuplicateDataException : Exception
    {
#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        public IReadOnlyCollection<IReadOnlyEntityType> EntityTypes { get; }
#else
        // IEntityType
        public IReadOnlyCollection<IEntityType> EntityTypes { get; }
#endif

#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        public DuplicateDataException(IReadOnlyEntityType[] entityTypes)
#else
        // IEntityType
        public DuplicateDataException(IEntityType[] entityTypes)
#endif
            : base(entityTypes.Aggregate(
                new StringBuilder(),
                (sb, et) => sb.AppendLine($"Duplicates detected for {et}"),
                sb => sb.ToString()
                ))
        {
            this.EntityTypes = entityTypes;
        }
    }
}