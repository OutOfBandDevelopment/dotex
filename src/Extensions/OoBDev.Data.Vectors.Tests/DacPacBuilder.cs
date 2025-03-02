using Castle.Core.Internal;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace OoBDev.Data.Vectors.Tests;

[TestClass]
public class DacPacBuilderTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void BuildPackageTest()
    {
        var sqlClrAssembly = typeof(CentroidFAggregator).Assembly;
        var builder = new DacPacBuilder();
        var xml = builder.BuildPackage(sqlClrAssembly);
        xml.Save("model.xml");
    }
}

public class DacPacBuilder
{
    private readonly XNamespace ns = "http://schemas.microsoft.com/sqlserver/dac/Serialization/2012/02";

    public XElement BuildPackage(Assembly assembly)
    {
        var dataSchemaModel = new XElement(
            ns + "DataSchemaModel",
            new XAttribute("FileFormatVersion", "1.2"),
            new XAttribute("SchemaVersion", "2.9"),
            new XAttribute("DspName", "Microsoft.Data.Tools.Schema.Sql.Sql150DatabaseSchemaProvider"),
            new XAttribute("CollationLcid", "1033"),
            new XAttribute("CollationCaseSensitive", "False"),

            XElement.Parse(@"<Header xmlns=""http://schemas.microsoft.com/sqlserver/dac/Serialization/2012/02"">
		<CustomData Category=""AnsiNulls"">
			<Metadata Name=""AnsiNulls"" Value=""True"" />
		</CustomData>
		<CustomData Category=""QuotedIdentifier"">
			<Metadata Name=""QuotedIdentifier"" Value=""True"" />
		</CustomData>
		<CustomData Category=""CompatibilityMode"">
			<Metadata Name=""CompatibilityMode"" Value=""150"" />
		</CustomData>
		<CustomData Category=""Reference"" Type=""Assembly"">
			<Metadata Name=""LogicalName"" Value=""OoBDev.Data.Vectors.dll"" />
			<Metadata Name=""FileName"" Value=""C:\REPOS\TEMP\DATABASE1\DATABASE1\OBJ\DEBUG\OOBDEV.DATA.VECTORS.DLL"" />
			<Metadata Name=""AssemblyName"" Value=""OoBDev.Data.Vectors"" />
			<Metadata Name=""PermissionSet"" Value=""SAFE"" />
			<Metadata Name=""Owner"" Value="""" />
			<Metadata Name=""GenerateSqlClrDdl"" Value=""True"" />
			<Metadata Name=""IsVisible"" Value=""True"" />
			<Metadata Name=""IsModelAware"" Value=""True"" />
			<Metadata Name=""SkipCreationIfEmpty"" Value=""True"" />
			<Metadata Name=""AssemblySymbolsName"" Value=""C:\Repos\temp\Database1\Database1\obj\Debug\OoBDev.Data.Vectors.pdb"" />
		</CustomData>
		<CustomData Category=""SqlCmdVariables"" Type=""SqlCmdVariable"" />
	</Header>")
            );

        var metaData = dataSchemaModel.Descendants(ns + "Metadata");

        var logicalName = metaData.FirstOrDefault(x => ((string)x.Attribute("Name")) == "LogicalName");
        logicalName?.Attribute("Value").SetValue(Path.GetFileName(assembly.Location));
        var fileName = metaData.FirstOrDefault(x => ((string)x.Attribute("Name")) == "FileName");
        fileName?.Attribute("Value").SetValue(assembly.Location);

        var realAssemblyName = assembly.FullName.Split(',').First();
        var assemblyName = metaData.FirstOrDefault(x => ((string)x.Attribute("Name")) == "AssemblyName");
        assemblyName?.Attribute("Value").SetValue(realAssemblyName);
        var assemblySymbolsName = metaData.FirstOrDefault(x => ((string)x.Attribute("Name")) == "AssemblySymbolsName");
        assemblySymbolsName?.Attribute("Value").SetValue(Path.ChangeExtension(assembly.Location, ".pdb"));

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

        model.Add(Aggregates(ns, assembly, realAssemblyName));
        model.Add(Functions(ns, assembly, realAssemblyName));

        return dataSchemaModel;
    }

    public IEnumerable<XElement> Aggregates(XNamespace ns, Assembly assembly, string realAssemblyName) =>
        from type in assembly.GetTypes()
        let attrib = type.GetAttributes<SqlUserDefinedAggregateAttribute>().FirstOrDefault()
        let accumulator = type.GetMethod("Accumulate")
        let terminator = type.GetMethod("Terminate")
        where attrib != null
        select new XElement(ns + "Element", new XAttribute("Type", "SqlAggregate"), new XAttribute("Name", attrib.Name), //TODO: I need a name builder
           new XElement(ns + "Property", new XAttribute("Name", "Format"), new XAttribute("Value", (int)attrib.Format)),
           new XElement(ns + "Property", new XAttribute("Name", "IsInvariantToDuplicates"), new XAttribute("Value", attrib.IsInvariantToDuplicates ? "True" : "False")),
           new XElement(ns + "Property", new XAttribute("Name", "IsInvariantToNulls"), new XAttribute("Value", attrib.IsInvariantToNulls ? "True" : "False")),
           new XElement(ns + "Property", new XAttribute("Name", "IsNullIfEmpty"), new XAttribute("Value", attrib.IsNullIfEmpty ? "True" : "False")),
           new XElement(ns + "Property", new XAttribute("Name", "MaxByteSize"), new XAttribute("Value", attrib.MaxByteSize)),
           new XElement(ns + "Property", new XAttribute("Name", "ClassName"), new XAttribute("Value", type.Name)),
           new XElement(ns + "Relationship", new XAttribute("Name", "Assembly"),
               new XElement(ns + "Entry",
                   new XElement(ns + "References", new XAttribute("Name", $"[{realAssemblyName}]")
                   )
               )
           ),
          Parameters(accumulator.GetParameters()),
          Return(terminator.ReturnParameter),
          Schema(type)
       );

    public string? GetName(object input) =>
        input switch
        {
            ParameterInfo parameter => $"{GetName(parameter.Member) ?? GetName(parameter.Member.DeclaringType)}.[@{parameter.Name}]",
            Type type => type.GetAttributes<SqlUserDefinedAggregateAttribute>().FirstOrDefault()?.Name ??
                         type.GetAttributes<SqlUserDefinedTypeAttribute>().FirstOrDefault()?.Name ??
                         _typeName.GetValueOrDefault(type) ??
                         GetTypeName(type),
            MethodInfo method => method.GetAttributes<SqlFunctionAttribute>().FirstOrDefault()?.Name,
            _ => null
        };


    private readonly IReadOnlyDictionary<Type, string> _typeName = new Dictionary<Type, string>
    {
        { typeof(SqlByte), "TINYINT" },
        { typeof(SqlInt16), "SMALLINT" },
        { typeof(SqlInt32), "INT" },
        { typeof(SqlInt64), "BIGINT" },
        { typeof(SqlBytes ), "VARBINARY(max)" },
        { typeof(SqlBinary ), "VARBINARY(max)" },
        { typeof(SqlBoolean), "BIT" },
        { typeof(SqlDateTime), "DATETIME" },
        { typeof(SqlDecimal), "DECIMAL(29,4)" },
        { typeof(SqlDouble), "FLOAT" },
        { typeof(SqlSingle), "REAL" },
        { typeof(SqlString), "NVARCHAR(MAX)" },
        { typeof(SqlXml), "XML" },
        { typeof(SqlChars), "NVARCHAR(MAX)" },
        { typeof(SqlGuid), "UNIQUEIDENTIFIER" },
        { typeof(SqlGeography), "geography" },
        { typeof(SqlHierarchyId), "HIERARCHYID" },
        { typeof(SqlGeometry), "geometry" },

        { typeof(char), "NCHAR(1)" },
        { typeof(sbyte), "SMALLINT" },
        { typeof(byte), "TINYINT" },
        { typeof(short), "SMALLINT" },
        { typeof(int), "INT" },
        { typeof(long), "BIGINT" },
        { typeof(ushort), "INT" },
        { typeof(uint), "BIGINT" },
        { typeof(ulong), "DECIMAL(20)" },
        { typeof(decimal), "DECIMAL(29,4)" },
        { typeof(float), "REAL" },
        { typeof(double), "FLOAT" },
        { typeof(DateTime), "DATETIME2" },
        { typeof(DateTimeOffset), "DATETIMEOFFSET" },
        { typeof(TimeSpan), "TIME" },
        { typeof(Guid), "UNIQUEIDENTIFIER" },

        { typeof(char?), "NCHAR(1)" },
        { typeof(sbyte?), "SMALLINT" },
        { typeof(byte?), "TINYINT" },
        { typeof(short?), "SMALLINT" },
        { typeof(int?), "INT" },
        { typeof(long?), "BIGINT" },
        { typeof(ushort?), "INT" },
        { typeof(uint?), "BIGINT" },
        { typeof(ulong?), "DECIMAL(20)" },
        { typeof(decimal?), "DECIMAL(29,4)" },
        { typeof(float?), "REAL" },
        { typeof(double?), "FLOAT" },
        { typeof(DateTime?), "DATETIME2" },
        { typeof(DateTimeOffset?), "DATETIMEOFFSET" },
        { typeof(TimeSpan?), "TIME" },
        { typeof(Guid?), "UNIQUEIDENTIFIER" },

        { typeof(string), "NVARCHAR(MAX)" },
        { typeof(char[]), "NVARCHAR(MAX)" },
    };
    private string GetTypeName(Type type) => throw new NotSupportedException($"no mapping found for {type}");

    public XElement Schema(object input) =>
        new XElement(ns + "Relationship", new XAttribute("Name", "Schema"),
            new XElement(ns + "Entry",
                    new XElement(ns + "References",
                        new XAttribute("ExternalSource", "BuiltIns"),
                        new XAttribute("Name", GetName(input).Split('.')[0])
                )
            )
        );

    public XElement Parameters(IEnumerable<ParameterInfo> parameters) =>
        new XElement(ns + "Relationship", new XAttribute("Name", "Parameters"),
            from parameter in parameters
            select new XElement(ns + "Entry",
                new XElement(ns + "Element", new XAttribute("Type", "SqlSubroutineParameter"), new XAttribute("Name", GetName(parameter)),
                    new XElement(ns + "Relationship", new XAttribute("Name", "Type"),
                        new XElement(ns + "Entry",
                            new XElement(ns + "Element", new XAttribute("Type", "SqlTypeSpecifier"),
                                new XElement(ns + "Relationship", new XAttribute("Name", "Type"),
                                    new XElement(ns + "Entry",
                                        new XElement(ns + "References", new XAttribute("Name", GetName(parameter.ParameterType))
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            )
        );

    public XElement Return(ParameterInfo returnInfo) =>
        new XElement(ns + "Relationship", new XAttribute("Name", "ReturnType"),
            new XElement(ns + "Entry",
                new XElement(ns + "Element", new XAttribute("Type", "SqlTypeSpecifier"),
                        new XElement(ns + "Relationship", new XAttribute("Name", "Type"),
                            new XElement(ns + "Entry",
                                new XElement(ns + "Element", new XAttribute("Type", "SqlTypeSpecifier"),
                                    new XElement(ns + "Relationship", new XAttribute("Name", "Type"),
                                        new XElement(ns + "Entry",
                                            new XElement(ns + "References", new XAttribute("Name", GetName(returnInfo.ParameterType))
                                            )
                                        )
                                    )
                                )
                            )
                        )
                )
            )
        );

    public IEnumerable<XElement> Functions(XNamespace ns, Assembly assembly, string realAssemblyName) =>
        from functionClasses in assembly.GetTypes().Where(t => t.IsAbstract)
        from function in functionClasses.GetMethods(BindingFlags.Static | BindingFlags.Public)
        let attrib = function.GetAttributes<SqlFunctionAttribute>().FirstOrDefault()
        where attrib != null
        select new XElement(ns + "Element",
            new XAttribute("Type", function.ReturnType.IsAssignableTo(typeof(IEnumerable)) ? throw new NotSupportedException($"{function.ReturnType}") : "SqlScalarFunction"),
            new XAttribute("Name", attrib.Name),
            new XElement(ns + "Property", new XAttribute("Name", "IsAnsiNullsOn")),
            new XElement(ns + "Property", new XAttribute("Name", "IsQuotedIdentifierOn")),
            new XElement(ns + "Relationship", new XAttribute("Name", "FunctionBody"),
                new XElement(ns + "Entry",
                    new XElement(ns + "Element", new XAttribute("Type", "SqlClrFunctionImplementation"),
                        new XElement(ns + "Property", new XAttribute("Name", "IsDeterministic"), new XAttribute("Value", attrib.IsDeterministic ? "True" : "False")),
                        new XElement(ns + "Property", new XAttribute("Name", "IsPrecise"), new XAttribute("Value", attrib.IsPrecise ? "True" : "False")),
                        new XElement(ns + "Property", new XAttribute("Name", "MethodName"), new XAttribute("Value", function.Name)),
                        new XElement(ns + "Property", new XAttribute("Name", "ClassName"), new XAttribute("Value", functionClasses.Name)),
                        new XElement(ns + "Relationship", new XAttribute("Name", "Assembly"),
                            new XElement(ns + "Entry",
                                new XElement(ns + "References", new XAttribute("Name", $"[{realAssemblyName}]"))
                            )
                        )
                    )
                )
            ),
            Parameters(function.GetParameters()),
            Schema(function),
            Return(function.ReturnParameter)
        );
}
