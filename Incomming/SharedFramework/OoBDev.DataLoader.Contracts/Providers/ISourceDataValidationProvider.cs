using OoBDev.DataLoader.Models;
using System.Collections.Generic;

namespace OoBDev.DataLoader.Providers
{
    public interface ISourceDataValidationProvider
    {
        IEnumerable<ValidEntityTypeModel> Validate(IDatabaseDeploymentTemplate template, IEnumerable<SourceEntityTypeReferenceModel> sourceEntities);
    }

}