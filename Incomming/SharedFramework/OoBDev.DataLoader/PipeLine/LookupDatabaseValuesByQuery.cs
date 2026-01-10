using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Linq;

namespace OoBDev.DataLoader.PipeLine
{
    /// <summary>
    /// This extensions provides the ability to perform secondary lookups based on SQL queries.
    /// 
    /// The columns returned from the query will be added as values to the input jtoken
    /// 
    /// <code>
    /// {
    ///     "__Lookup": {
    ///         "Query": "SELECT TOP (1) [DocumentIdentity] FROM [Core].[Documents] WHERE [DocumentId] = '00086AA3-995F-4A92-A9E2-F91F611B253B'"
    ///     }
    /// }
    /// </code>
    /// ..or..
    /// <code>
    /// {
    ///     "__Lookup.Query": "SELECT TOP (1) [DocumentIdentity] FROM [Core].[Documents] WHERE [DocumentId] = '00086AA3-995F-4A92-A9E2-F91F611B253B'"
    /// }
    /// </code>
    /// ..or..
    /// <code>
    /// {
    ///     "__Lookup_Query": "SELECT TOP (1) [DocumentIdentity] FROM [Core].[Documents] WHERE [DocumentId] = '00086AA3-995F-4A92-A9E2-F91F611B253B'"
    /// }
    /// </code>
    /// </summary>
    [DataPipelinePriority(Priority)]
    public class LookupDatabaseValuesByQuery : IDataPipelineHandler
    {
        public const int Priority = -10;

        private readonly ILogger _logger;

        public LookupDatabaseValuesByQuery(
             ILogger<LookupDatabaseValuesByQuery> logger
            )
        {
            _logger = logger;
        }

#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        public JToken Handle(DbContext context, IReadOnlyEntityType entityType, JToken input)
#else
        // IEntityType
        public JToken Handle(DbContext context, IEntityType entityType, JToken input)
#endif
        {
            /*
    "__Lookup": {
      "Query": "SELECT TOP (1) [DocumentIdentity] FROM [Core].[Documents] WHERE [DocumentId] = '00086AA3-995F-4A92-A9E2-F91F611B253B'"
    }
             */
            var lookup = input["__Lookup"]?["Query"] ?? input["__Lookup.Query"] ?? input["__Lookup_Query"];
            if (lookup != null && ((string?)lookup) is string queryString && !string.IsNullOrWhiteSpace(queryString))
            {
                _logger.LogInformation($"Execute query for {{entityType}}: {{query}}", entityType, queryString);

                var conn = context.Database.GetDbConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = queryString;
                using var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                if (reader.Read()) //Note: only look up the first value
                {
                    var map = from i in Enumerable.Range(0, reader.FieldCount)
                              select new
                              {
                                  Name = reader.GetName(i),
                                  Value = reader.GetValue(i),
                              };
                    var values = map.ToArray();
                    foreach (var item in values)
                    {
                        if (string.IsNullOrWhiteSpace(item.Name)) continue;

                        input[item.Name] = item.Value == null || item.Value == DBNull.Value ? null : JToken.FromObject(item.Value);
                    }
                }

                lookup.Parent?.Remove();
            }
            return input;
        }
    }
}
