using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json.Linq;

namespace OoBDev.DataLoader.PipeLine
{
    public interface IDataPipelineHandler
    {
#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        JToken Handle(DbContext context, IReadOnlyEntityType entityType, JToken input);
#else
        // IEntityType
        JToken Handle(DbContext context, IEntityType entityType, JToken input);
#endif
    }
}
