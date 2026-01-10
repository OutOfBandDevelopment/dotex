using OoBDev.ComplexEvents.Contracts.Schedulers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OoBDev.ComplexEvents.EntityFrameworkCore
{
    public class SchedulePersistenceProvider<TDbContext> : ISchedulePersistenceProvider
         where TDbContext : DbContext
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<TDbContext> _log;

        public SchedulePersistenceProvider(
            IServiceProvider services,
            ILogger<TDbContext> log
            )
        {
            _services = services;
            _log = log;
        }

        private DbContext DB()
        {
            var db = ActivatorUtilities.CreateInstance<TDbContext>(_services);
            db.Database.AutoTransactionsEnabled = false;
            return db;
        }

        public async Task<IGetScheduleInstance?> GetAndLockAsync()
        {
            try
            {
                using var db = DB();

                //check if there is anything to process
                // UPDLOCK is required to let SQL know you intend on getting a lock on this record.  
                // READPAD is required to let SQL know you want to skip records locked by other processes/threads

                using var cmd = db.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = SchedulePersistenceProviderCommands.GetRecord;
                cmd.CommandType = CommandType.StoredProcedure;
                await db.Database.OpenConnectionAsync();
                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    // Nothing to do
                    return null;
                }

                var eventGeneratorId = reader.GetInt32(0);
                var assemblyQualifiedName = reader.GetString(1);
                var type = Type.GetType(assemblyQualifiedName);

                if (type == null)
                {
                    return null;
                }

                var originalSchedule = reader.GetString(2);
                var schedulesXml = reader.GetTextReader(3)?.ReadToEnd();

                var schedules =
                    !string.IsNullOrWhiteSpace(schedulesXml) ?
                    XElement.Parse(schedulesXml).Elements("S").Select(x => (string)x).ToArray() :
                    new[] { originalSchedule };
                await reader.CloseAsync();

                using var update = db.Database.GetDbConnection().CreateCommand();
                update.CommandText = SchedulePersistenceProviderCommands.LockRecord;
                update.CommandType = CommandType.StoredProcedure;
                var egi = update.CreateParameter();
                egi.ParameterName = "@eventGeneratorId";
                egi.Value = eventGeneratorId;
                egi.Direction = ParameterDirection.Input;
                egi.DbType = DbType.Int32;
                update.Parameters.Add(egi);

                await db.Database.OpenConnectionAsync();
                await update.ExecuteNonQueryAsync().ConfigureAwait(false);


                _log.LogInformation($"{nameof(GetAndLockAsync)}: {assemblyQualifiedName} ({eventGeneratorId})");

                return new GetScheduleInstance(eventGeneratorId.ToString(), type, schedules);
            }
            catch (Exception ex)
            {
                _log.LogError($"{nameof(GetAndLockAsync)}: {ex.Message}");
                _log.LogDebug($"{nameof(GetAndLockAsync)}: {ex}");
                throw;
            }
        }

        public async Task<int> GetPendingCountAsync()
        {
            try
            {
                using var db = DB();
                using var cmd = db.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = SchedulePersistenceProviderCommands.PendingCount;
                cmd.CommandType = CommandType.StoredProcedure;
                await db.Database.OpenConnectionAsync();
                var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false) switch
                {
                    int value => value,
                    _ => 0
                };

                _log.LogInformation($"{nameof(GetPendingCountAsync)}: {result}");

                return result;
            }
            catch (Exception ex)
            {
                _log.LogError($"{nameof(GetPendingCountAsync)}: {ex.Message}");
                _log.LogDebug($"{nameof(GetPendingCountAsync)}: {ex}");
                throw;
            }
        }

        public async Task RegisterIfNotExistAsync(IEnumerable<IRegisterScheduleInstance> schedules)
        {
            try
            {
                var schedulesX = new XElement("EventGenerators",
                from eventGenerator in schedules
                where eventGenerator.Scheduler != null
                where eventGenerator.Schedules != null
                select new XElement("EventGenerator",
                    new XAttribute("AssemblyName", eventGenerator.Scheduler.Assembly.FullName.Split(',').FirstOrDefault()),
                    new XAttribute("Namespace", eventGenerator.Scheduler.Namespace),
                    new XAttribute("TypeName", eventGenerator.Scheduler.Name),
                    eventGenerator.NextStart.HasValue ? new XAttribute("NextStart", eventGenerator.NextStart) : null,
                    from schedule in eventGenerator.Schedules
                    select new XElement("Schedule", schedule)
                    )
                );
                var schedulesXml = schedulesX.ToString();

                using var db = DB();

                using var update = db.Database.GetDbConnection().CreateCommand();
                update.CommandText = SchedulePersistenceProviderCommands.RegisterSchedulers;
                update.CommandType = CommandType.StoredProcedure;

                var next = update.CreateParameter();
                next.ParameterName = "@xml";
                next.Value = schedulesXml;
                next.Direction = ParameterDirection.Input;
                next.DbType = DbType.Xml;
                update.Parameters.Add(next);

                await db.Database.OpenConnectionAsync();
                var updated = await update.ExecuteNonQueryAsync().ConfigureAwait(false);

                _log.LogInformation($"{nameof(RegisterIfNotExistAsync)}: {updated}");
            }
            catch (Exception ex)
            {
                _log.LogError($"{nameof(RegisterIfNotExistAsync)}: {ex.Message}");
                _log.LogDebug($"{nameof(RegisterIfNotExistAsync)}: {ex}");
                throw;
            }
        }

        public async Task ReleaseAsync(IReleaseScheduleInstance instance)
        {
            try
            {
                using var db = DB();

                using var update = db.Database.GetDbConnection().CreateCommand();
                update.CommandText = SchedulePersistenceProviderCommands.ReleaseLock;
                update.CommandType = CommandType.StoredProcedure;

                var next = update.CreateParameter();
                next.ParameterName = "@NextRun";
                next.Value = instance.NextStart;
                next.Direction = ParameterDirection.Input;
                next.DbType = DbType.DateTimeOffset;
                update.Parameters.Add(next);

                var lem = update.CreateParameter();
                lem.ParameterName = "@LastErrorMessage";
                lem.Value = string.IsNullOrWhiteSpace(instance.ErrorMessage) ? (object)DBNull.Value : instance.ErrorMessage;
                lem.Direction = ParameterDirection.Input;
                lem.DbType = DbType.String;
                lem.SourceColumnNullMapping = true;
                update.Parameters.Add(lem);

                var egi = update.CreateParameter();
                egi.ParameterName = "@eventGeneratorId";
                egi.Value = int.TryParse(instance.ReferenceKey, out var egiv) ? egiv : 0;
                egi.Direction = ParameterDirection.Input;
                egi.DbType = DbType.Int32;
                update.Parameters.Add(egi);

                await db.Database.OpenConnectionAsync();
                await update.ExecuteNonQueryAsync().ConfigureAwait(false);

                _log.LogInformation($"{nameof(ReleaseAsync)}: {instance.Scheduler}({instance.ReferenceKey}) Next:{instance.NextStart} Result:{instance.ErrorMessage}");
            }
            catch (Exception ex)
            {
                _log.LogError($"{nameof(ReleaseAsync)}: {ex.Message}");
                _log.LogDebug($"{nameof(ReleaseAsync)}: {ex}");
                throw;
            }
        }
    }
}
