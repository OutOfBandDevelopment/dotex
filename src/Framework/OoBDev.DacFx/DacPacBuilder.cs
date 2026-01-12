// Ignore Spelling: Dac
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace OoBDev.DacFx;
public class DacPacBuilder : IDacPacBuilder
{
    private readonly ILogger _logger;
    private readonly IDacPacValidator _validator;
    private readonly XNamespace ns = "http://schemas.microsoft.com/sqlserver/dac/Serialization/2012/02";

    public DacPacBuilder(
        ILogger<DacPacBuilder> logger,
        IDacPacValidator validator
        )
    {
        _logger = logger;
        _validator = validator;
    }

    #region MetadataLoadContext Setup

    private static string GetNet48ReferenceAssembliesPath()
    {
        var programFiles86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        // Try reference assemblies first (best for metadata)
        var refAssemblies = Path.Combine(programFiles86, "Reference Assemblies", "Microsoft", "Framework", ".NETFramework", "v4.8.1");
        if (Directory.Exists(refAssemblies))
            return refAssemblies;

        // Fallback to v4.8
        refAssemblies = Path.Combine(programFiles86, "Reference Assemblies", "Microsoft", "Framework", ".NETFramework", "v4.8");
        if (Directory.Exists(refAssemblies))
            return refAssemblies;

        // Last resort: runtime directory (GAC)
        var windir = Environment.GetEnvironmentVariable("WINDIR") ?? @"C:\Windows";
        return Path.Combine(windir, "Microsoft.NET", "Framework64", "v4.0.30319");
    }

    private static MetadataLoadContext GetMetadataLoadContext(string assemblyPath)
    {
        var runtimeDir = GetNet48ReferenceAssembliesPath();
        var assemblyDir = Path.GetDirectoryName(assemblyPath)!;

        var paths = Directory.GetFiles(runtimeDir, "*.dll")
            .Concat(Directory.GetFiles(assemblyDir, "*.dll"))
            .Distinct();

        var resolver = new PathAssemblyResolver(paths);
        return new MetadataLoadContext(resolver, "mscorlib");
    }

    #endregion

    #region Public API

    public void BuildDacPac(
        string assemblyFileFramework,
        string? assemblyPdbFramework = null,
        string? dacpacFile = null,
        string? projectName = null,
        string? projectVersion = null)
    {
        if (string.IsNullOrWhiteSpace(assemblyFileFramework))
            throw new ArgumentNullException(nameof(assemblyFileFramework));

        using var mlc = GetMetadataLoadContext(assemblyFileFramework);
        var sqlClrAssembly = mlc.LoadFromAssemblyPath(assemblyFileFramework);

        var bothPath = string.IsNullOrWhiteSpace(dacpacFile);
        assemblyPdbFramework = Path.GetFullPath(assemblyPdbFramework ?? Path.ChangeExtension(assemblyFileFramework, ".pdb"));
        dacpacFile = Path.GetFullPath(dacpacFile ?? Path.ChangeExtension(assemblyFileFramework, ".dacpac"));
        projectName ??= Path.GetFileNameWithoutExtension(assemblyFileFramework);
        projectVersion ??= "0.0.0.1";

        _logger.LogInformation("Building DACPAC: {projectName} v{projectVersion}", projectName, projectVersion);
        _logger.LogInformation("Assembly: {assemblyFileFramework}", assemblyFileFramework);
        _logger.LogInformation("Output: {dacpacFile}", dacpacFile);

        if (File.Exists(dacpacFile))
            File.Delete(dacpacFile);

        using (var archive = ZipFile.Open(dacpacFile, ZipArchiveMode.Create))
        {
            var modelXml = BuildModel(sqlClrAssembly, assemblyFileFramework, assemblyPdbFramework);

#if DEBUG
            // Save model.xml for debugging
            var debugModelPath = Path.ChangeExtension(dacpacFile, ".model.xml");
            modelXml.Save(debugModelPath);
            _logger.LogInformation("DEBUG: Saved model.xml to {debugModelPath}", debugModelPath);
#endif 

            var modelHash = AddXmlToArchive(archive, "model.xml", modelXml);

            var originXml = BuildOrigin(modelHash);
            AddXmlToArchive(archive, "Origin.xml", originXml);

            var dacMetadataXml = BuildDacMetadata(projectName, projectVersion);
            AddXmlToArchive(archive, "DacMetadata.xml", dacMetadataXml);

            var contentTypeXml = BuildContentType();
            AddXmlToArchive(archive, "[Content_Types].xml", contentTypeXml);
        }

        _logger.LogInformation("DACPAC created successfully: {dacpacFile}", dacpacFile);

        // Validate the DACPAC using Microsoft DacFx
        _validator.ValidateDacPac(dacpacFile);

        if (bothPath)
        {
            var dacpacFile2 = Path.GetFullPath(Path.ChangeExtension(assemblyFileFramework, ".dacpac"));
            if (dacpacFile != dacpacFile2)
            {
                File.Copy(dacpacFile, dacpacFile2, overwrite: true);
                _logger.LogInformation("Copied to: {dacpacFile2}", dacpacFile2);
            }
        }
    }
    #endregion

    #region Archive Helpers

    private string AddXmlToArchive(ZipArchive archive, string entryName, XElement xml)
    {
        using var stream = new MemoryStream();
        xml.Save(stream);
        stream.Position = 0;

        var entry = archive.CreateEntry(entryName);
        entry.LastWriteTime = DateTimeOffset.Now;

        using (var entryStream = entry.Open())
        {
            stream.CopyTo(entryStream);
        }

        stream.Position = 0;
        return GetSha256(stream.ToArray());
    }

    #endregion

    #region XML Builders

    public XElement BuildModel(Assembly assembly, string assemblyFile, string? pdbFile)
    {
        var realAssemblyName = assembly.FullName?.Split(',').First()
            ?? throw new ApplicationException("Assembly name is required");

        var dataSchemaModel = new XElement(
            ns + "DataSchemaModel",
            new XAttribute("FileFormatVersion", "1.2"),
            new XAttribute("SchemaVersion", "2.9"),
            new XAttribute("DspName", "Microsoft.Data.Tools.Schema.Sql.Sql150DatabaseSchemaProvider"),
            new XAttribute("CollationLcid", "1033"),
            new XAttribute("CollationCaseSensitive", "False")
        );

        var model = new XElement(ns + "Model",
            XElement.Parse(@"<Element Type=""SqlDatabaseOptions"" xmlns=""http://schemas.microsoft.com/sqlserver/dac/Serialization/2012/02"">
                <Property Name=""Collation"" Value=""SQL_Latin1_General_CP1_CI_AS"" />
                <Property Name=""IsAnsiNullDefaultOn"" Value=""True"" />
                <Property Name=""IsAnsiNullsOn"" Value=""True"" />
                <Property Name=""IsAnsiWarningsOn"" Value=""True"" />
                <Property Name=""IsArithAbortOn"" Value=""True"" />
                <Property Name=""IsConcatNullYieldsNullOn"" Value=""True"" />
                <Property Name=""IsTornPageProtectionOn"" Value=""False"" />
                <Property Name=""IsFullTextEnabled"" Value=""True"" />
                <Property Name=""PageVerifyMode"" Value=""3"" />
                <Property Name=""DefaultLanguage"" Value="""" />
                <Property Name=""DefaultFullTextLanguage"" Value="""" />
                <Property Name=""QueryStoreStaleQueryThreshold"" Value=""367"" />
                <Relationship Name=""DefaultFilegroup"">
                    <Entry>
                        <References ExternalSource=""BuiltIns"" Name=""[PRIMARY]"" />
                    </Entry>
                </Relationship>
            </Element>")
        );

        dataSchemaModel.Add(model);
        model.Add(Files(realAssemblyName, assemblyFile, pdbFile));
        model.Add(Aggregates(assembly, realAssemblyName));
        model.Add(UserDefinedTypes(assembly, realAssemblyName));
        model.Add(Functions(assembly, realAssemblyName));
        model.Add(CollectSchema(dataSchemaModel));

        return dataSchemaModel;
    }

    public XElement BuildOrigin(string modelHash)
    {
        var xml = XElement.Parse(@"<?xml version=""1.0"" encoding=""utf-8""?>
<DacOrigin xmlns=""http://schemas.microsoft.com/sqlserver/dac/Serialization/2012/02"">
  <PackageProperties>
    <Version>3.0.0.0</Version>
    <ContainsExportedData>false</ContainsExportedData>
    <StreamVersions>
      <Version StreamName=""Data"">2.0.0.0</Version>
      <Version StreamName=""DeploymentContributors"">1.0.0.0</Version>
    </StreamVersions>
  </PackageProperties>
  <Operation>
    <Identity>8836d3ee-a491-424c-8924-7772671badb6</Identity>
    <Start>2025-03-02T01:04:47.6699800-05:00</Start>
    <End>2025-03-02T01:04:47.7931242-05:00</End>
    <ProductName>Microsoft.Data.Tools.Schema.Tasks.Sql, Version=162.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a</ProductName>
    <ProductVersion>162.6.15.1</ProductVersion>
    <ProductSchema>http://schemas.microsoft.com/sqlserver/dac/Serialization/2012/02</ProductSchema>
  </Operation>
  <Checksums>
    <Checksum Uri=""/model.xml""></Checksum>
  </Checksums>
  <ModelSchemaVersion>2.9</ModelSchemaVersion>
</DacOrigin>");

        xml.Descendants(ns + "Checksum").First().SetValue(modelHash);
        return xml;
    }

    public XElement BuildDacMetadata(string projectName, string versionNumber)
    {
        var xml = XElement.Parse(@"<?xml version=""1.0"" encoding=""utf-8""?>
<DacType xmlns=""http://schemas.microsoft.com/sqlserver/dac/Serialization/2012/02"">
  <Name>Database1</Name>
  <Version>1.0.0.0</Version>
</DacType>");

        xml.Descendants(ns + "Name").First().SetValue(projectName);
        xml.Descendants(ns + "Version").First().SetValue(versionNumber);
        return xml;
    }

    public XElement BuildContentType() =>
        XElement.Parse(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""xml"" ContentType=""text/xml"" />
</Types>");

    #endregion

    #region SQL CLR Element Extractors

    /// <summary>
    /// Checks if a type implements IEnumerable by examining interfaces via FullName.
    /// Required for MetadataLoadContext compatibility - runtime type comparisons don't work.
    /// </summary>
    private static bool IsEnumerableType(Type type) =>
        type.GetInterfaces().Any(i =>
            i.FullName == "System.Collections.IEnumerable" ||
            i.FullName == "System.Collections.Generic.IEnumerable`1");

    public IEnumerable<XElement> Aggregates(Assembly assembly, string realAssemblyName)
    {
        foreach (var type in assembly.GetTypes())
        {
            var attrData = CustomAttributeData.GetCustomAttributes(type)
                .FirstOrDefault(a => a.AttributeType.FullName == "Microsoft.SqlServer.Server.SqlUserDefinedAggregateAttribute");

            if (attrData == null)
                continue;

            var name = GetAttributeName(attrData);
            _logger.LogDebug("Processing aggregate: {typeName}, AttributeName: '{name}'", type.FullName, name ?? "(null)");

            if (string.IsNullOrWhiteSpace(name))
                throw new NotSupportedException($"SqlUserDefinedAggregate on {type.FullName} must have a non-empty Name");

            var accumulator = type.GetMethod("Accumulate");
            var terminator = type.GetMethod("Terminate");
            var format = GetNamedArgument<int>(attrData, "Format");
            var isInvariantToDuplicates = GetNamedArgument<bool>(attrData, "IsInvariantToDuplicates");
            var isInvariantToNulls = GetNamedArgument<bool>(attrData, "IsInvariantToNulls");
            var isNullIfEmpty = GetNamedArgument<bool>(attrData, "IsNullIfEmpty");
            var maxByteSize = GetNamedArgument<int>(attrData, "MaxByteSize");

            yield return new XElement(ns + "Element",
                new XAttribute("Type", "SqlAggregate"),
                new XAttribute("Name", name),
                new XElement(ns + "Property", new XAttribute("Name", "Format"), new XAttribute("Value", format)),
                new XElement(ns + "Property", new XAttribute("Name", "IsInvariantToDuplicates"), new XAttribute("Value", isInvariantToDuplicates ? "True" : "False")),
                new XElement(ns + "Property", new XAttribute("Name", "IsInvariantToNulls"), new XAttribute("Value", isInvariantToNulls ? "True" : "False")),
                new XElement(ns + "Property", new XAttribute("Name", "IsNullIfEmpty"), new XAttribute("Value", isNullIfEmpty ? "True" : "False")),
                new XElement(ns + "Property", new XAttribute("Name", "MaxByteSize"), new XAttribute("Value", maxByteSize)),
                new XElement(ns + "Property", new XAttribute("Name", "ClassName"), new XAttribute("Value", type.FullName!)),
                new XElement(ns + "Relationship", new XAttribute("Name", "Assembly"),
                    new XElement(ns + "Entry",
                        new XElement(ns + "References", new XAttribute("Name", $"[{realAssemblyName}]"))
                    )
                ),
                FunctionParameters(accumulator!.GetParameters()),
                Return(terminator!.ReturnParameter, isFunction: false),
                Schema(name)
            );
        }
    }

    public IEnumerable<XElement> UserDefinedTypes(Assembly assembly, string realAssemblyName)
    {
        foreach (var type in assembly.GetTypes())
        {
            var attrData = CustomAttributeData.GetCustomAttributes(type)
                .FirstOrDefault(a => a.AttributeType.FullName == "Microsoft.SqlServer.Server.SqlUserDefinedTypeAttribute");

            if (attrData == null)
                continue;

            var name = GetAttributeName(attrData);
            _logger.LogDebug("Processing UDT: {typeName}, AttributeName: '{name}'", type.FullName, name ?? "(null)");

            if (string.IsNullOrWhiteSpace(name))
                throw new NotSupportedException($"SqlUserDefinedType on {type.FullName} must have a non-empty Name");

            var format = GetNamedArgument<int>(attrData, "Format");
            var maxByteSize = GetNamedArgument<int>(attrData, "MaxByteSize");
            var isByteOrdered = GetNamedArgument<bool>(attrData, "IsByteOrdered");

            yield return new XElement(ns + "Element",
                new XAttribute("Type", "SqlUserDefinedType"),
                new XAttribute("Name", name),
                new XElement(ns + "Property", new XAttribute("Name", "Format"), new XAttribute("Value", format)),
                new XElement(ns + "Property", new XAttribute("Name", "MaxByteSize"), new XAttribute("Value", maxByteSize)),
                new XElement(ns + "Property", new XAttribute("Name", "IsByteOrdered"), new XAttribute("Value", isByteOrdered ? "True" : "False")),
                new XElement(ns + "Property", new XAttribute("Name", "ClassName"), new XAttribute("Value", type.FullName!)),
                new XElement(ns + "Relationship", new XAttribute("Name", "Assembly"),
                    new XElement(ns + "Entry",
                        new XElement(ns + "References", new XAttribute("Name", $"[{realAssemblyName}]"))
                    )
                ),
                Methods(assembly, realAssemblyName, type),
                Schema(name)
            );
        }
    }

    public IEnumerable<XElement> Functions(Assembly assembly, string realAssemblyName)
    {
        foreach (var functionClasses in assembly.GetTypes().Where(t => t.IsAbstract))
        {
            foreach (var function in functionClasses.GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                var attrData = CustomAttributeData.GetCustomAttributes(function)
                    .FirstOrDefault(a => a.AttributeType.FullName == "Microsoft.SqlServer.Server.SqlFunctionAttribute");

                if (attrData == null)
                    continue;

                var functionName = GetAttributeName(attrData);
                _logger.LogDebug("Processing function: {className}.{methodName}, AttributeName: '{name}'",
                    functionClasses.FullName, function.Name, functionName ?? "(null)");

                if (string.IsNullOrWhiteSpace(functionName))
                    throw new NotSupportedException($"SqlFunction on {functionClasses.FullName}.{function.Name} must have a non-empty Name");

                var isDeterministic = GetNamedArgument<bool>(attrData, "IsDeterministic");
                var isPrecise = GetNamedArgument<bool>(attrData, "IsPrecise");

                if (IsEnumerableType(function.ReturnType))
                    throw new NotSupportedException($"Table-valued functions not supported: {function.ReturnType}");

                yield return new XElement(ns + "Element",
                    new XAttribute("Type", "SqlScalarFunction"),
                    new XAttribute("Name", functionName),
                    new XElement(ns + "Property", new XAttribute("Name", "IsAnsiNullsOn"), new XAttribute("Value", "True")),
                    new XElement(ns + "Property", new XAttribute("Name", "IsQuotedIdentifierOn"), new XAttribute("Value", "True")),
                    new XElement(ns + "Relationship", new XAttribute("Name", "FunctionBody"),
                        new XElement(ns + "Entry",
                            new XElement(ns + "Element", new XAttribute("Type", "SqlClrFunctionImplementation"),
                                new XElement(ns + "Property", new XAttribute("Name", "IsDeterministic"), new XAttribute("Value", isDeterministic ? "True" : "False")),
                                new XElement(ns + "Property", new XAttribute("Name", "IsPrecise"), new XAttribute("Value", isPrecise ? "True" : "False")),
                                new XElement(ns + "Property", new XAttribute("Name", "MethodName"), new XAttribute("Value", function.Name)),
                                new XElement(ns + "Property", new XAttribute("Name", "ClassName"), new XAttribute("Value", functionClasses.FullName!)),
                                new XElement(ns + "Relationship", new XAttribute("Name", "Assembly"),
                                    new XElement(ns + "Entry",
                                        new XElement(ns + "References", new XAttribute("Name", $"[{realAssemblyName}]"))
                                    )
                                )
                            )
                        )
                    ),
                    FunctionParameters(function.GetParameters()),
                    Schema(functionName),
                    Return(function.ReturnParameter, isFunction: true)
                );
            }
        }
    }

    public XElement Methods(Assembly assembly, string realAssemblyName, Type sqlClrType)
    {
        var typeName = GetName(sqlClrType) ?? throw new NotSupportedException($"Type {sqlClrType.FullName} must have a Name");

        return new XElement(ns + "Relationship", new XAttribute("Name", "Methods"),
            from function in sqlClrType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            let attrData = CustomAttributeData.GetCustomAttributes(function)
                .FirstOrDefault(a => a.AttributeType.FullName == "Microsoft.SqlServer.Server.SqlFunctionAttribute")
            where attrData != null
            let functionName = GetName(function) ?? throw new NotSupportedException($"Method {function.Name} on {sqlClrType.FullName} must have a Name")
            select new XElement(ns + "Entry",
                new XElement(ns + "Element",
                    new XAttribute("Type", "SqlClrMethod"),
                    new XAttribute("Name", $"{typeName}.[{functionName}]"),
                    new XElement(ns + "Property",
                        new XAttribute("Name", "ClrName"),
                        new XAttribute("Value", functionName)),
                    MethodParameters(function.GetParameters()),
                    Return(function.ReturnParameter, isFunction: false)
                )
            )
        );
    }

    public IEnumerable<XElement> Files(string realAssemblyName, string assemblyFile, string? pdbFile)
    {
        if (File.Exists(assemblyFile))
        {
            yield return new XElement(ns + "Element",
                new XAttribute("Type", "SqlAssembly"),
                new XAttribute("Name", $"[{realAssemblyName}]"),
                new XElement(ns + "Relationship", new XAttribute("Name", "AssemblySources"),
                    new XElement(ns + "Entry",
                        new XElement(ns + "Element", new XAttribute("Type", "SqlAssemblySource"),
                            new XElement(ns + "Property", new XAttribute("Name", "Source"),
                                new XElement(ns + "Value",
                                    new XCData($"0x{GetHexContent(assemblyFile)}")
                                )
                            )
                        )
                    )
                ),
                new XElement(ns + "Relationship", new XAttribute("Name", "Authorizer"),
                    new XElement(ns + "Entry",
                        new XElement(ns + "References",
                            new XAttribute("ExternalSource", "BuiltIns"),
                            new XAttribute("Name", "[dbo]"))
                    )
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(pdbFile) && File.Exists(pdbFile))
        {
            yield return new XElement(ns + "Element",
                new XAttribute("Type", "SqlAssemblyFile"),
                new XAttribute("Name", $"[{realAssemblyName}].[{Path.GetFileName(pdbFile)}]"),
                new XElement(ns + "Property", new XAttribute("Name", "Source"),
                    new XElement(ns + "Value",
                        new XCData($"0x{GetHexContent(pdbFile)}")
                    )
                ),
                new XElement(ns + "Relationship", new XAttribute("Name", "Assembly"),
                    new XElement(ns + "Entry",
                        new XElement(ns + "References", new XAttribute("Name", $"[{realAssemblyName}]"))
                    )
                )
            );
        }
    }

    #endregion

    #region Parameter and Type Handling

    public XElement FunctionParameters(IEnumerable<ParameterInfo> parameters) =>
        new XElement(ns + "Relationship", new XAttribute("Name", "Parameters"),
            from parameter in parameters
            select new XElement(ns + "Entry",
                new XElement(ns + "Element",
                    new XAttribute("Type", "SqlSubroutineParameter"),
                    new XAttribute("Name", GetName(parameter) ?? throw new NotSupportedException()),
                    new XElement(ns + "Relationship", new XAttribute("Name", "Type"),
                        new XElement(ns + "Entry",
                            TypeSpecifier(parameter)
                        )
                    )
                )
            )
        );

    public XElement? MethodParameters(IEnumerable<ParameterInfo> parameters)
    {
        var paramArray = parameters.ToArray();
        if (paramArray.Length == 0)
            return null;

        return new XElement(ns + "Relationship", new XAttribute("Name", "Parameters"),
            from parameter in paramArray
            let typeName = GetName(parameter.Member.DeclaringType) ?? throw new NotSupportedException($"Type {parameter.Member.DeclaringType?.FullName} must have a Name")
            let methodName = GetName(parameter.Member) ?? throw new NotSupportedException($"Method {parameter.Member.Name} must have a Name")
            select new XElement(ns + "Entry",
                new XElement(ns + "Element",
                    new XAttribute("Type", "SqlClrMethodParameter"),
                    new XAttribute("Name", $"{typeName}.[{methodName}].[{parameter.Name}]"),
                    new XElement(ns + "Property",
                        new XAttribute("Name", "ClrName"),
                        new XAttribute("Value", parameter.Name!)),
                    new XElement(ns + "Relationship", new XAttribute("Name", "Type"),
                        new XElement(ns + "Entry",
                            TypeSpecifier(parameter)
                        )
                    )
                )
            )
        );
    }

    public XElement Return(ParameterInfo returnInfo, bool isFunction) =>
        new XElement(ns + "Relationship",
            new XAttribute("Name", isFunction ? "Type" : "ReturnType"),
            new XElement(ns + "Entry",
                TypeSpecifier(returnInfo)
            )
        );

    public XElement TypeSpecifier(ParameterInfo parameterInfo) =>
        new XElement(ns + "Element", new XAttribute("Type", "SqlTypeSpecifier"),
            Properties(parameterInfo),
            new XElement(ns + "Relationship", new XAttribute("Name", "Type"),
                new XElement(ns + "Entry",
                    new XElement(ns + "References",
                        ExternalSource(parameterInfo.ParameterType),
                        new XAttribute("Name", GetName(parameterInfo.ParameterType) ?? throw new NotSupportedException())
                    )
                )
            )
        );

    public IEnumerable<XElement> Properties(ParameterInfo parameterInfo)
    {
        var fullName = parameterInfo.ParameterType.FullName;

        if (fullName == null) yield break;

        // IsMax for string/binary types
        if (_isMax.Contains(fullName))
        {
            yield return new XElement(ns + "Property",
                new XAttribute("Name", "IsMax"),
                new XAttribute("Value", "True"));
        }

        // Precision for doubles
        if (_doubles.Contains(fullName))
        {
            yield return new XElement(ns + "Property",
                new XAttribute("Name", "Precision"),
                new XAttribute("Value", "53"));
        }
    }

    #endregion

    #region Schema and Naming

    private IEnumerable<XElement> CollectSchema(XElement dataSchemaModel) =>
        from schema in dataSchemaModel.Descendants(ns + "Relationship")
            .Where(x => (string?)x.Attribute("Name") == "Schema")
            .SelectMany(x => x.Descendants(ns + "References"))
            .Select(x => (string?)x.Attribute("Name"))
            .Distinct()
        select new XElement(ns + "Element",
            new XAttribute("Type", "SqlSchema"),
            new XAttribute("Name", schema!),
            new XElement(ns + "Relationship", new XAttribute("Name", "Authorizer"),
                new XElement(ns + "Entry",
                    new XElement(ns + "References",
                        new XAttribute("ExternalSource", "BuiltIns"),
                        new XAttribute("Name", "[dbo]"))
                )
            )
        );
    public XElement Schema(object input)
    {
        if (input is not string fullName)
            fullName = GetName(input) ?? throw new NotSupportedException($"Cannot get name for schema from {input?.GetType().Name ?? "null"}");

        if (string.IsNullOrWhiteSpace(fullName) || fullName == "[]")
            throw new NotSupportedException($"Schema name cannot be empty or just brackets: '{fullName}'");

        // Extract schema name: [embedding].[Centroid] -> [embedding]
        var schemaName = fullName.Contains('.')
            ? fullName.Substring(0, fullName.LastIndexOf('.'))
            : "[dbo]";

        // Validate schema name is not empty
        if (string.IsNullOrWhiteSpace(schemaName) || schemaName == "[]")
            schemaName = "[dbo]";

        return new XElement(ns + "Relationship", new XAttribute("Name", "Schema"),
            new XElement(ns + "Entry",
                new XElement(ns + "References",
                    new XAttribute("Name", schemaName)
                )
            )
        );
    }

    public string? GetName(object? input) =>
        input switch
        {
            ParameterInfo parameter => $"{GetName(parameter.Member) ?? GetName(parameter.Member.DeclaringType)}.[@{parameter.Name}]",
            Type type => GetTypeName(type),
            MethodInfo method => GetMethodName(method),
            _ => null
        };

    private string? GetMethodName(MethodInfo method)
    {
        var attrData = CustomAttributeData.GetCustomAttributes(method)
            .FirstOrDefault(a => a.AttributeType.FullName == "Microsoft.SqlServer.Server.SqlFunctionAttribute");

        if (attrData != null)
        {
            var name = GetAttributeName(attrData);
            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }

        // Return null for methods without SqlFunction attribute
        // This allows parameter names to fall back to using the declaring type name
        return null;
    }

    private string? GetTypeName(Type type)
    {
        // Check for SQL CLR attributes first
        var aggregateAttr = CustomAttributeData.GetCustomAttributes(type)
            .FirstOrDefault(a => a.AttributeType.FullName == "Microsoft.SqlServer.Server.SqlUserDefinedAggregateAttribute");
        if (aggregateAttr != null)
        {
            var name = GetAttributeName(aggregateAttr);
            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }

        var typeAttr = CustomAttributeData.GetCustomAttributes(type)
            .FirstOrDefault(a => a.AttributeType.FullName == "Microsoft.SqlServer.Server.SqlUserDefinedTypeAttribute");
        if (typeAttr != null)
        {
            var name = GetAttributeName(typeAttr);
            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }

        // Check type mapping dictionary
        var mappedName = GetTypeNameFromDictionary(type);
        if (!string.IsNullOrWhiteSpace(mappedName))
            return mappedName;

        // Default to full type name
        if (string.IsNullOrWhiteSpace(type.FullName))
            return null;

        return $"[{type.FullName}]";
    }

    #endregion

    #region Attribute Helpers

    private static T? GetNamedArgument<T>(CustomAttributeData attrData, string name)
    {
        var arg = attrData.NamedArguments.FirstOrDefault(a => a.MemberName == name);
        return arg.TypedValue.Value != null ? (T)arg.TypedValue.Value : default;
    }

    private static string? GetAttributeName(CustomAttributeData attrData)
    {
        // Try named argument
        var nameArg = attrData.NamedArguments.FirstOrDefault(a => a.MemberName == "Name");
        if (nameArg.MemberName != null)
        {
            var name = nameArg.TypedValue.Value?.ToString();
            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }

        // Try constructor argument
        if (attrData.ConstructorArguments.Count > 0)
        {
            var name = attrData.ConstructorArguments[0].Value?.ToString();
            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }

        return null;
    }

    #endregion

    #region Type Mapping

    private static readonly IReadOnlyDictionary<string, string> _typeName = new Dictionary<string, string>
    {
        { "System.Data.SqlTypes.SqlByte", "[tinyint]" },
        { "System.Data.SqlTypes.SqlInt16", "[smallint]" },
        { "System.Data.SqlTypes.SqlInt32", "[int]" },
        { "System.Data.SqlTypes.SqlInt64", "[bigint]" },
        { "System.Data.SqlTypes.SqlBytes", "[varbinary]" },
        { "System.Data.SqlTypes.SqlBinary", "[varbinary]" },
        { "System.Data.SqlTypes.SqlBoolean", "[bit]" },
        { "System.Data.SqlTypes.SqlDateTime", "[datetime2]" },
        { "System.Data.SqlTypes.SqlDecimal", "[decimal(29,4)]" },
        { "System.Data.SqlTypes.SqlDouble", "[float]" },
        { "System.Data.SqlTypes.SqlSingle", "[real]" },
        { "System.Data.SqlTypes.SqlString", "[nvarchar]" },
        { "System.Data.SqlTypes.SqlXml", "[xml]" },
        { "System.Data.SqlTypes.SqlChars", "[nvarchar]" },
        { "System.Data.SqlTypes.SqlGuid", "[uniqueidentifier]" },
        { "Microsoft.SqlServer.Types.SqlGeography", "[geography]" },
        { "Microsoft.SqlServer.Types.SqlHierarchyId", "[hierarchyid]" },
        { "Microsoft.SqlServer.Types.SqlGeometry", "[geometry]" },
        { "System.Char", "[nchar(1)]" },
        { "System.SByte", "[smallint]" },
        { "System.Byte", "[tinyint]" },
        { "System.Int16", "[smallint]" },
        { "System.Int32", "[int]" },
        { "System.Int64", "[bigint]" },
        { "System.UInt16", "[int]" },
        { "System.UInt32", "[bigint]" },
        { "System.UInt64", "[decimal(20)]" },
        { "System.Decimal", "[decimal(29,4)]" },
        { "System.Single", "[real]" },
        { "System.Double", "[float]" },
        { "System.DateTime", "[datetime2]" },
        { "System.DateTimeOffset", "[datetimeoffset]" },
        { "System.TimeSpan", "[time]" },
        { "System.Guid", "[uniqueidentifier]" },
        { "System.String", "[nvarchar]" },
        { "System.Object", "[sql_variant]" },
    };

    private static readonly HashSet<string> _isMax = new()
    {
        "System.Data.SqlTypes.SqlString",
        "System.String",
        "System.Byte[]",
        "System.Char[]"
    };

    private static readonly HashSet<string> _doubles = new()
    {
        "System.Data.SqlTypes.SqlDouble",
        "System.Double"
    };

    private string? GetTypeNameFromDictionary(Type type)
    {
        // Handle nullable types (check by FullName for MetadataLoadContext compatibility)
        var lookupType = type;
        if (type.IsGenericType && type.GetGenericTypeDefinition().FullName == "System.Nullable`1")
        {
            lookupType = type.GetGenericArguments()[0];
        }

        // Handle arrays
        if (lookupType.IsArray)
        {
            var elementType = lookupType.GetElementType();
            if (elementType?.FullName == "System.Char")
                return "[nvarchar]";
            if (elementType?.FullName == "System.Byte")
                return "[varbinary]";
        }

        return _typeName.GetValueOrDefault(lookupType.FullName ?? "");
    }

    private XAttribute? ExternalSource(Type type)
    {
        var fullName = type.FullName;
        if (fullName != null && _typeName.ContainsKey(fullName))
            return new XAttribute("ExternalSource", "BuiltIns");
        return null;
    }

    #endregion

    #region Utility Methods

    public string? GetHexContent(string file) =>
        BitConverter.ToString(File.ReadAllBytes(file)).Replace("-", "");

    public string GetSha256(string file) =>
        GetSha256(File.ReadAllBytes(file));

    public string GetSha256(byte[] content) =>
        BitConverter.ToString(SHA256.HashData(content)).Replace("-", "");

    public string GetSha512(string file) =>
        GetSha512(File.ReadAllBytes(file));

    public string GetSha512(byte[] content) =>
        BitConverter.ToString(SHA512.HashData(content)).Replace("-", "");

    #endregion
}
