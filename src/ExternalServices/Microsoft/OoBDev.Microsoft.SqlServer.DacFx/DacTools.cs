using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;

namespace OoBDev.Microsoft.SqlServer.DacFx;

/// <summary>
/// Provides utility methods for working with DacPac files and SQL Server database models.
/// </summary>
public static class DacTools
{
    /// <summary>
    /// Reads the entire contents of a stream as a string.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <returns>The stream contents as a string, or null if the stream is null or empty.</returns>
    public static string? ReadToEnd(this Stream? stream)
    {
        if (stream == null) return null;
        using var reader = new StreamReader(stream);
        var read = reader.ReadToEnd();
        if (string.IsNullOrWhiteSpace(read)) return null;
        return read;
    }

    /// <summary>
    /// Extracts the model options from a DacPac file.
    /// </summary>
    /// <param name="filename">The path to the DacPac file.</param>
    /// <returns>The model options from the DacPac.</returns>
    public static TSqlModelOptions GetModelOptions(string filename)
    {
        using var sqlModel = new TSqlModel(filename);
        var options = sqlModel.CopyModelOptions();
        return options;
    }

    /// <summary>
    /// Opens a DacPac file and returns its TSqlModel.
    /// </summary>
    /// <param name="filename">The path to the DacPac file.</param>
    /// <returns>A TSqlModel representing the DacPac contents.</returns>
    public static TSqlModel OpenDacPacModel(string filename) => new(filename);

    /// <summary>
    /// Reads all user-defined objects from a TSqlModel as name/script pairs.
    /// </summary>
    /// <param name="sqlModel">The TSqlModel to read from.</param>
    /// <returns>An enumerable of tuples containing object names and their TSqlScript definitions.</returns>
    public static IEnumerable<(string name, TSqlScript script)> ReadPackage(this TSqlModel sqlModel)
    {
        var objs = sqlModel.GetObjects(DacQueryScopes.UserDefined);
        foreach (var obj in objs)
        {
            if (!obj.TryGetAst(out var ast)) continue;

            var name = obj.GetSourceInformation()?.SourceName;
            if (string.IsNullOrWhiteSpace(name)) name = $"{obj.Name}_{obj.ObjectType.Name}";
            if (string.IsNullOrWhiteSpace(name) || name.EndsWith(".xsd", StringComparison.InvariantCultureIgnoreCase)) continue;

            yield return (name, ast);
        }
    }

    /// <summary>
    /// Extracts the pre-deployment and post-deployment scripts from a DacPac file.
    /// </summary>
    /// <param name="fileName">The path to the DacPac file.</param>
    /// <returns>A tuple containing the pre-deployment and post-deployment scripts, or null if not present.</returns>
    public static (string? PreDeploymentScript, string? PostDeploymentScript) GetScripts(string fileName)
    {
        using var dac = DacPackage.Load(fileName, DacSchemaModelStorageType.File);
        return (
            dac.PreDeploymentScript.ReadToEnd(),
            dac.PostDeploymentScript.ReadToEnd()
            );
    }

    /// <summary>
    /// Adds pre-deployment and post-deployment scripts to an existing DacPac file.
    /// </summary>
    /// <param name="file">The path to the DacPac file.</param>
    /// <param name="scripts">A tuple containing the pre-deployment and post-deployment scripts to add.</param>
    public static void AddScripts(string file, (string? preDeployment, string? postDeployment) scripts)
    {
        using var package = Package.Open(file, FileMode.Open, FileAccess.ReadWrite);
        package.AddFileContent("/predeploy.sql", scripts.preDeployment)
               .AddFileContent("/postdeploy.sql", scripts.postDeployment)
               .Close()
               ;
    }

    /// <summary>
    /// Adds a file with text content to a package.
    /// </summary>
    /// <param name="package">The package to add the file to.</param>
    /// <param name="path">The relative path within the package.</param>
    /// <param name="content">The text content to write, or null to skip.</param>
    /// <param name="contentType">The MIME content type. Default is "text/plain".</param>
    /// <returns>The package for method chaining.</returns>
    public static Package AddFileContent(this Package package, string path, string? content, string contentType = "text/plain")
    {
        if (!string.IsNullOrEmpty(content))
        {
            var part = package.CreatePart(new Uri(path, UriKind.Relative), contentType);

            using var stream = part.GetStream();
            using var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
        }
        return package;
    }

    /// <summary>
    /// Generates a T-SQL script that sets or updates the database version extended property.
    /// </summary>
    /// <param name="version">The version string to set.</param>
    /// <returns>A T-SQL script that sets the DbVersion extended property.</returns>
    public static string GenerateBuildVersionScript(string? version) => $@"
IF EXISTS (
	SELECT *
	FROM SYS.EXTENDED_PROPERTIES
	WHERE
		[major_id] = 0
		AND [name] = N'DbVersion'
		AND [minor_id] = 0
)
BEGIN
EXEC sp_updateextendedproperty
	@name='DbVersion',
	@value ='{version}',
	@level0type = NULL,
	@level0name = NULL,
	@level1type = NULL,
	@level1name = NULL,
	@level2type = NULL,
	@level2name = NULL;
END
ELSE 
BEGIN
EXEC sp_addextendedproperty
	@name='DbVersion',
	@value ='{version}',
	@level0type = NULL,
	@level0name = NULL,
	@level1type = NULL,
	@level1name = NULL,
	@level2type = NULL,
	@level2name = NULL;
END
PRINT 'BuildVersion = {version}';
";

}
