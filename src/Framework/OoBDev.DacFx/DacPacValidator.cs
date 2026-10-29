// Ignore Spelling: Dac
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using System;
using System.Linq;

namespace OoBDev.DacFx;

/// <summary>
/// Validates DACPAC files using Microsoft DacFx APIs.
/// </summary>
public partial class DacPacValidator : IDacPacValidator
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DacPacValidator"/> class.
    /// </summary>
    /// <param name="logger">The logger for diagnostic output.</param>
    public DacPacValidator(ILogger<DacPacValidator> logger) => _logger = logger;

    /// <summary>
    /// Validates the generated DACPAC using Microsoft.SqlServer.DacFx APIs
    /// </summary>
    public void ValidateDacPac(string dacpacFile)
    {
        LogValidatingDacpacWithMicrosoft(dacpacFile);

        try
        {
            // Load the DACPAC using official DacFx
            using (var package = DacPackage.Load(dacpacFile))
            {
                LogDacpacLoadedSuccessfullyBy();
                LogName(package.Name);
                LogVersion(package.Version);
                LogDescription(package.Description ?? "(none)");
            }
            // Load the TSqlModel to validate the schema
            using (var model = new TSqlModel(dacpacFile))
            {
                LogTsqlmodelLoadedSuccessfully();

                LogModelContents(model);
            }
        }
        catch (Exception ex)
        {
            LogDacpacValidationFailed(ex);
            throw new InvalidOperationException($"Generated DACPAC is invalid: {ex.Message}", ex);
        }
    }

    private void LogModelContents(TSqlModel model)
    {
        // Count objects in the model
        var assemblies = model.GetObjects(DacQueryScopes.All, ModelSchema.Assembly).ToList();
        var functions = model.GetObjects(DacQueryScopes.All, ModelSchema.ScalarFunction).ToList();
        var aggregates = model.GetObjects(DacQueryScopes.All, ModelSchema.Aggregate).ToList();
        var udts = model.GetObjects(DacQueryScopes.All, ModelSchema.UserDefinedType).ToList();

        LogDacpacContents();
        LogAssemblies(assemblies.Count);
        LogScalarFunctions(functions.Count);
        LogAggregates(aggregates.Count);
        LogUserDefinedTypes(udts.Count);

        // Validate each assembly
        foreach (var assembly in assemblies)
        {
            var name = assembly.Name.ToString();
            LogAssembly(name);
        }

        // Validate each function
        foreach (var function in functions)
        {
            var name = function.Name.ToString();
            LogFunction(name);
        }

        // Validate each aggregate
        foreach (var aggregate in aggregates)
        {
            var name = aggregate.Name.ToString();
            LogAggregate(name);
        }

        // Validate each UDT
        foreach (var udt in udts)
        {
            var name = udt.Name.ToString();
            LogUdt(name);
        }

        LogDacpacValidationSuccessful();
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Validating DACPAC with Microsoft DacFx: {dacpacFile}")]
    private partial void LogValidatingDacpacWithMicrosoft(string? dacpacFile);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "DACPAC loaded successfully by DacFx")]
    private partial void LogDacpacLoadedSuccessfullyBy();

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "  Name: {name}")]
    private partial void LogName(string? name);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "  Version: {version}")]
    private partial void LogVersion(Version version);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "  Description: {description}")]
    private partial void LogDescription(string? description);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "TSqlModel loaded successfully")]
    private partial void LogTsqlmodelLoadedSuccessfully();

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "DACPAC contents:")]
    private partial void LogDacpacContents();

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "  Assemblies: {count}")]
    private partial void LogAssemblies(int count);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "  Scalar Functions: {count}")]
    private partial void LogScalarFunctions(int count);

    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "  Aggregates: {count}")]
    private partial void LogAggregates(int count);

    [LoggerMessage(EventId = 11, Level = LogLevel.Information, Message = "  User-Defined Types: {count}")]
    private partial void LogUserDefinedTypes(int count);

    [LoggerMessage(EventId = 12, Level = LogLevel.Information, Message = "  Assembly: {name}")]
    private partial void LogAssembly(string? name);

    [LoggerMessage(EventId = 13, Level = LogLevel.Information, Message = "  Function: {name}")]
    private partial void LogFunction(string? name);

    [LoggerMessage(EventId = 14, Level = LogLevel.Information, Message = "  Aggregate: {name}")]
    private partial void LogAggregate(string? name);

    [LoggerMessage(EventId = 15, Level = LogLevel.Information, Message = "  UDT: {name}")]
    private partial void LogUdt(string? name);

    [LoggerMessage(EventId = 16, Level = LogLevel.Information, Message = "DACPAC validation successful!")]
    private partial void LogDacpacValidationSuccessful();

    [LoggerMessage(EventId = 17, Level = LogLevel.Error, Message = "DACPAC validation failed")]
    private partial void LogDacpacValidationFailed(Exception exception);
}
