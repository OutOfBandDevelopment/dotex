using OoBDev.DataLoader.DataReaders;
using OoBDev.DataLoader.Models;
using OoBDev.DataLoader.PipeLine;
using OoBDev.EntityFrameworkCore;
using OoBDev.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.DataLoader.Providers
{
    public class SourceDataPipeline : ISourceDataPipeline
    {
        private readonly IMasterDataProvider _masterDataProvider;
        private readonly IDataFileProvider _dataFileProvider;
        private readonly IDataFileReaderProvider _dataFileReaderProvider;
        private readonly IDataPipelineProcessor _dataPipelineProcessor;

        public SourceDataPipeline(
            IMasterDataProvider masterDataProvider,
            IDataFileProvider dataFileProvider,
            IDataFileReaderProvider dataFileReaderProvider,
            IDataPipelineProcessor dataPipelineProcessor
            )
        {
            _masterDataProvider = masterDataProvider;
            _dataFileProvider = dataFileProvider;
            _dataFileReaderProvider = dataFileReaderProvider;
            _dataPipelineProcessor = dataPipelineProcessor;
        }

#if NET5_0_OR_GREATER
        public IEnumerable<SourceEntityTypeReferenceModel> ReadEntityData(IDatabaseDeploymentTemplate template, DbContext dbContext, IEnumerable<IReadOnlyEntityType> entityTypes) =>
#else
        public IEnumerable<SourceEntityTypeReferenceModel> ReadEntityData(IDatabaseDeploymentTemplate template, DbContext dbContext, IEnumerable<IEntityType> entityTypes) =>
#endif

             from entityType in entityTypes

             let pk = entityType.FindPrimaryKey()
             let defaultPk = pk.GetDefaultValues().AsTuple()

             let masterData = from data in _masterDataProvider.GetMasterData(entityType)
                              select new SourceDataReferenceModel
                              {
                                  IsMasterData = true,
                                  Data = data,
                                  Reference = entityType.ClrType.AssemblyQualifiedName,
                              }

             let dataRecords = from sourcePath in template.SourceDataPaths
                               from dataFile in _dataFileProvider.GetDataFiles(sourcePath, entityType)
                               from data in _dataFileReaderProvider.ReadFile(entityType, dataFile)
                               select new SourceDataReferenceModel
                               {
                                   IsMasterData = false,
                                   Data = data,
                                   Reference = dataFile,
                               }

             let dataSet = template.ImportMasterData switch
             {
                 ImportHandling.Skip => dataRecords,
                 ImportHandling.Last => dataRecords.Concat(masterData),
                 ImportHandling.First => masterData.Concat(dataRecords),
                 _ => masterData.Concat(dataRecords),
             }

             let entities = from item in dataSet
                            let entity = _dataPipelineProcessor.ConvertToEntity(dbContext, entityType, item.Data)
                            let pkd = pk.GetValuesFrom(entity)
                            let pkv = pkd.AsTuple()
                            select new SourceEntityReferenceModel
                            {
                                Source = item,
                                KeyData = pkd,
                                KeyValue = pkv,
                                Entity = entity,
                            }

             let grouped = from entity in entities
                           group entity by entity.KeyValue

             select new SourceEntityTypeReferenceModel
             {
                 DbContext = dbContext,
                 DefaultPrimaryKey = defaultPk,
                 EntityType = entityType,
                 Entities = grouped,
                 HasDuplicates = grouped.Any(i => i.Key != defaultPk && i.Count() > 1),
             };
    }
}
