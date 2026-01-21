using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json.Linq;

namespace OoBDev.DataLoader.PipeLine
{
    public interface ILookupAlternativeKey
    {
#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        JToken? Existing(DbContext context, IReadOnlyEntityType entityType, JToken input, IReadOnlyKey key);
#else
        // IEntityType
        JToken? Existing(DbContext context, IEntityType entityType, JToken input, IKey key);
#endif
    }
}
