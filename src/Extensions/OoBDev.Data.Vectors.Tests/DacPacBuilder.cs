using Castle.Core.Internal;
using Microsoft.SqlServer.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}

public class DacPacBuilder
{
    public XElement BuildPackage(Assembly assembly)
    {
        XNamespace ns = "http://schemas.microsoft.com/sqlserver/dac/Serialization/2012/02";
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

        var aggregates = from type in assembly.GetTypes()
                         let attrib = type.GetAttributes<SqlUserDefinedAggregateAttribute>().FirstOrDefault()
                         where attrib != null
                         select new XElement(ns + "Element",
                            new XElement(ns + "Property", new XAttribute("Name", "Type"), new XAttribute("Value", "SqlAggregate"), new XAttribute("Name", attrib.Name)), //TODO: I need a name builder
                            new XElement(ns + "Property", new XAttribute("Name", "Format"), new XAttribute("Value", (int)attrib.Format)),
                            new XElement(ns + "Property", new XAttribute("Name", "IsInvariantToDuplicates"), new XAttribute("Value", attrib.IsInvariantToDuplicates ? "True" : "False")),
                            new XElement(ns + "Property", new XAttribute("Name", "IsInvariantToNulls"), new XAttribute("Value", attrib.IsInvariantToNulls ? "True" : "False")),
                            new XElement(ns + "Property", new XAttribute("Name", "IsNullIfEmpty"), new XAttribute("Value", attrib.IsNullIfEmpty ? "True" : "False")),
                            new XElement(ns + "Property", new XAttribute("Name", "MaxByteSize"), new XAttribute("Value", attrib.MaxByteSize)),
                            new XElement(ns + "Property", new XAttribute("Name", "ClassName"), new XAttribute("Value", type.Name)),
                            new XElement(ns + "Relationship", new XAttribute("Name", "Assembly"),
                                new XElement("Entry",
                                    new XElement("References", new XAttribute("Name", $"[{realAssemblyName}]")
                                    )
                                )
                            )
                        );
        model.Add(aggregates);

        return dataSchemaModel;
    }
}
