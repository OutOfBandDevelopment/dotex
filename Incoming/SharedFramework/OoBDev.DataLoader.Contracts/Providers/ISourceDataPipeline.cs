using OoBDev.DataLoader.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;

namespace OoBDev.DataLoader.Providers
{
    public interface ISourceDataPipeline
    {
#if NET5_0_OR_GREATER
        IEnumerable<SourceEntityTypeReferenceModel> ReadEntityData(IDatabaseDeploymentTemplate template, DbContext dbContext, IEnumerable<IReadOnlyEntityType> entityTypes);
#else
        IEnumerable < SourceEntityTypeReferenceModel> ReadEntityData(IDatabaseDeploymentTemplate template, DbContext dbContext, IEnumerable<IEntityType> entityTypes);
#endif
    }
}
