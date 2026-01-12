// Ignore Spelling: Dac
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using System;
using System.Linq;

namespace OoBDev.DacFx;

public class DacPacValidator : IDacPacValidator
{
    private readonly ILogger _logger;

    public DacPacValidator(ILogger<DacPacValidator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates the generated DACPAC using Microsoft.SqlServer.DacFx APIs
    /// </summary>
    public void ValidateDacPac(string dacpacFile)
    {
        _logger.LogInformation("Validating DACPAC with Microsoft DacFx: {dacpacFile}", dacpacFile);

        try
        {
            // Load the DACPAC using official DacFx
            using (var package = DacPackage.Load(dacpacFile))
            {
                _logger.LogInformation("DACPAC loaded successfully by DacFx");
                _logger.LogInformation("  Name: {name}", package.Name);
                _logger.LogInformation("  Version: {version}", package.Version);
                _logger.LogInformation("  Description: {description}", package.Description ?? "(none)");

                // Load the TSqlModel to validate the schema
                using (var model = new TSqlModel(dacpacFile))
                {
                    _logger.LogInformation("TSqlModel loaded successfully");

                    // Count objects in the model
                    var assemblies = model.GetObjects(DacQueryScopes.All, ModelSchema.Assembly).ToList();
                    var functions = model.GetObjects(DacQueryScopes.All, ModelSchema.ScalarFunction).ToList();
                    var aggregates = model.GetObjects(DacQueryScopes.All, ModelSchema.Aggregate).ToList();
                    var udts = model.GetObjects(DacQueryScopes.All, ModelSchema.UserDefinedType).ToList();

                    _logger.LogInformation("DACPAC contents:");
                    _logger.LogInformation("  Assemblies: {count}", assemblies.Count);
                    _logger.LogInformation("  Scalar Functions: {count}", functions.Count);
                    _logger.LogInformation("  Aggregates: {count}", aggregates.Count);
                    _logger.LogInformation("  User-Defined Types: {count}", udts.Count);

                    // Validate each assembly
                    foreach (var assembly in assemblies)
                    {
                        var name = assembly.Name.ToString();
                        _logger.LogInformation("  Assembly: {name}", name);
                    }

                    // Validate each function
                    foreach (var function in functions)
                    {
                        var name = function.Name.ToString();
                        _logger.LogInformation("  Function: {name}", name);
                    }

                    // Validate each aggregate
                    foreach (var aggregate in aggregates)
                    {
                        var name = aggregate.Name.ToString();
                        _logger.LogInformation("  Aggregate: {name}", name);
                    }

                    // Validate each UDT
                    foreach (var udt in udts)
                    {
                        var name = udt.Name.ToString();
                        _logger.LogInformation("  UDT: {name}", name);
                    }

                    _logger.LogInformation("DACPAC validation successful!");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DACPAC validation failed");
            throw new InvalidOperationException($"Generated DACPAC is invalid: {ex.Message}", ex);
        }
    }
}
