using OoBDev.DataLoader.Models;
using OoBDev.Toolkit.Common;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OoBDev.DataLoader
{
    public class DatabaseDeploymentTemplateFactory : IDatabaseDeploymentTemplateFactory
    {
        private readonly IDataloaderConfig _config;
        private readonly IObjectConverter _converter;
        private readonly ILogger<DatabaseDeploymentTemplateFactory> _logger;

        public DatabaseDeploymentTemplateFactory(
            IDataloaderConfig config,
            IObjectConverter converter,
            ILogger<DatabaseDeploymentTemplateFactory> logger
            )
        {
            _config = config;
            _converter = converter;
            _logger = logger;
        }

        public IDatabaseDeploymentTemplate GetTemplate() => GetTemplateAsync().GetAwaiter().GetResult();
        public async Task<IDatabaseDeploymentTemplate> GetTemplateAsync()
        {
            var template = await ReadTemplateFileAsync(_config.DeploymentDescriptionFile);
            template = ApplyOverridesToTemplate(template);
            template = EnsureTemplate(template);
            Log(template);
            return template;
        }

        private void Log(IDatabaseDeploymentTemplate obj)
        {
            if (obj == null) return;
            var ser = new YamlDotNet.Serialization.Serializer();
            var yml = ser.Serialize(obj);
            var der = new YamlDotNet.Serialization.Deserializer();
            var cp = der.Deserialize(yml, typeof(DatabaseDeploymentTemplate)) as DatabaseDeploymentTemplate;
            if (cp?.ConnectionString != null)
            {
                var csb = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(cp.ConnectionString);
                if (!string.IsNullOrWhiteSpace(csb.Password))
                    csb.Password = new string('*', csb.Password.Length);
                if (!string.IsNullOrWhiteSpace(csb.UserID))
                    csb.UserID = new string('*', csb.UserID.Length);
                if (!string.IsNullOrWhiteSpace(csb.WorkstationID))
                    csb.WorkstationID = new string('*', csb.WorkstationID.Length);
                cp.ConnectionString = csb.ToString();
                yml = ser.Serialize(cp);
            }
            _logger.LogInformation(yml);
        }

        private IDatabaseDeploymentTemplate ApplyOverridesToTemplate(IDatabaseDeploymentTemplate template) =>
            new DatabaseDeploymentTemplate
            {
                AssemblySearchPath = First(_config.SourceAssemblySearchPath, template.AssemblySearchPath),
                BasePath = First(_config.DeploymentBasePath, template.BasePath),
                ConnectionString = First(_config.TargetDatabaseConnectionString, template.ConnectionString),
                Contexts = First(_config.TargetContexts?.OfType<string>().ToArray(), template.Contexts) ?? Array.Empty<string>(),
                DisableForeignKeysBeforeInsert = First(_config.SettingsDisableForeignKeysBeforeInsert, template.DisableForeignKeysBeforeInsert) ?? true,
                DuplicateHandling = First(_config.SettingsDuplicateHandling, template.DuplicateHandling) ?? DuplicateHandling.Fail,
                ImportMasterData = First(_config.SettingsImportMasterData, template.ImportMasterData) ?? ImportHandling.First,
                SourceDataPaths = First(_config.SourceDataSearchPaths?.OfType<string>().ToArray(), template.SourceDataPaths) ?? Array.Empty<string>(),
                UserSession = new DataloaderUserSession
                {
                    Username = First(_config.DeploymentUserName, template.UserSession?.Username) ?? "",
                    UserId = First(_config.DeploymentUserId, template.UserSession?.UserId) ?? Guid.Empty,
                }
            };

        private T? First<T>(params T?[] items) where T : struct =>
            items.FirstOrDefault(i => i != null);
        private T? First<T>(params T?[] items) where T : class =>
            items.FirstOrDefault(i => i != null);

        private async Task<IDatabaseDeploymentTemplate> ReadTemplateFileAsync(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return new DatabaseDeploymentTemplate();
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"Missing Deployment Description File: \"{fileName}\"", fileName);

            var content = await File.ReadAllTextAsync(fileName).ConfigureAwait(false);
            var ext = Path.GetExtension(fileName).ToUpper();

            var template = ext switch
            {
                ".YML" => ReadAsYaml(content),
                ".YAML" => ReadAsYaml(content),
                _ => await _converter.ConvertAsync<DatabaseDeploymentTemplate>(content).ConfigureAwait(false)
            };

            return template ?? new DatabaseDeploymentTemplate();
        }

        private IDatabaseDeploymentTemplate EnsureTemplate(IDatabaseDeploymentTemplate? template)
        {
            var result = new DatabaseDeploymentTemplate();
            if (template != null)
            {
                if (!string.IsNullOrWhiteSpace(template.BasePath))
                {
                    var basePath = Path.GetFullPath(template.BasePath);
                    if (!Directory.Exists(basePath)) throw new ApplicationException($"Path: \"{template.BasePath}\" not found");
                    result.BasePath = basePath;

                    if (!string.IsNullOrWhiteSpace(template.AssemblySearchPath))
                    {
                        result.AssemblySearchPath = EnsurePathSave(basePath, template.AssemblySearchPath);
                    }

                    if (template.SourceDataPaths != null)
                    {
                        result.SourceDataPaths = template.SourceDataPaths.Select(p => EnsurePathSave(basePath, p)).ToArray();
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(template.AssemblySearchPath))
                        result.AssemblySearchPath = Path.GetFullPath(template.AssemblySearchPath);
                    if (template.SourceDataPaths != null)
                        result.SourceDataPaths = template.SourceDataPaths.Select(Path.GetFullPath).ToArray();
                }

                result.ImportMasterData = template.ImportMasterData;
                result.Contexts = template.Contexts;
                result.UserSession = template.UserSession;
                result.DuplicateHandling = template.DuplicateHandling;
                result.ConnectionString = template.ConnectionString;
            }
            return result;
        }

        private string EnsurePathSave(string basePath, string path)
        {
            var assemblyPath = Path.GetFullPath(Path.Combine(basePath, path));
            if (!Directory.Exists(assemblyPath)) throw new ApplicationException($"Path: \"{path}\" not found");
            if (!assemblyPath.StartsWith(basePath)) throw new ApplicationException($"Path: \"{path}\" is not a child of \"{basePath}\"");
            return assemblyPath;
        }

        private IDatabaseDeploymentTemplate ReadAsYaml(string content)
        {
            var yml = new YamlDotNet.Serialization.Deserializer();
            var deserialized = (DatabaseDeploymentTemplate)yml.Deserialize(content, typeof(DatabaseDeploymentTemplate));
            return deserialized;
        }
    }
}