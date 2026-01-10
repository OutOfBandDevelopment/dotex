using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace OoBDev.DataLoader.PipeLine
{
    [DataPipelinePriority(Priority)]
    public class LookupDatabaseValuesByProperty : IDataPipelineHandler
    {
        public const int Priority = -10;

        private readonly ILogger _logger;

        public LookupDatabaseValuesByProperty(
             ILogger<LookupDatabaseValuesByProperty> logger
            )
        {
            _logger = logger;
        }

#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        public JToken Handle(DbContext context, IReadOnlyEntityType entityType, JToken input)
#else
        public JToken Handle(DbContext context, IEntityType entityType, JToken input)
#endif
        {
            /*
__LookUp {
	Properties {
		RoleId:  { EntityType: "Role", Match: {Name:"Sample Admin"}  },
		RightId: { EntityType: "Right", Match: {Code:"Application_CORE.PERMISSIONS.CORE_DASHBOARD_VIEW"} }
	}
}

to resolve entity type
... Type.GetType() 
	if not found use currentType.GetType().Assembly.GetTypes().Where(f=>f.Name == entityType)
	if not found user the current entity type's namespace + "." + entityType

            */
            return input;
        }
    }
}
