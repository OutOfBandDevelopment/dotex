using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;

namespace OoBDev.Microsoft.SqlServer.DacFx;

public static class DacTools
{
    public static string? ReadToEnd(this Stream? stream)
    {
        if (stream == null) return null;
        using var reader = new StreamReader(stream);
        var read = reader.ReadToEnd();
        if (string.IsNullOrWhiteSpace(read)) return null;
        return read;
    }

    public static TSqlModelOptions GetModelOptions(string filename)
    {
        using var sqlModel = new TSqlModel(filename);
        var options = sqlModel.CopyModelOptions();
        return options;
    }

    public static TSqlModel OpenDacPacModel(string filename) => new TSqlModel(filename);

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

    public static (string? PreDeploymentScript, string? PostDeploymentScript) GetScripts(string fileName)
    {
        using var dac = DacPackage.Load(fileName, DacSchemaModelStorageType.File);
        return (
            dac.PreDeploymentScript.ReadToEnd(),
            dac.PostDeploymentScript.ReadToEnd()
            );
    }

    public static void AddScripts(string file, (string? preDeployment, string? postDeployment) scripts)
    {
        using var package = Package.Open(file, FileMode.Open, FileAccess.ReadWrite);
        package.AddFileContent("/predeploy.sql", scripts.preDeployment)
               .AddFileContent("/postdeploy.sql", scripts.postDeployment)
               .Close()
               ;
    }

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
