using Castle.Core.Internal;
using Microsoft.SqlServer.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    public IEnumerable<XElement> Aggregates(XNamespace ns, Assembly assembly, string realAssemblyName)
    {
        var aggregates = from type in assembly.GetTypes()
                         let attrib = type.GetAttributes<SqlUserDefinedAggregateAttribute>().FirstOrDefault()
                         let method = type.GetMethod("Accumulate")
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
                           Parameters(method.GetParameters()),
                            //new XElement(ns + "Relationship", new XAttribute("Name", "Parameters"),
                            //    new XElement(ns + "Entry",
                            //        new XElement(ns + "Element", new XAttribute("Type", "SqlSubroutineParameter"), new XAttribute("Name", $"{attrib.Name}.[@{method.GetParameters()[0].Name}]"), //TODO: I need a name builder
                            //             new XElement(ns + "Relationship", new XAttribute("Name", "Type"),
                            //                 new XElement(ns + "Entry",
                            //                     new XElement(ns + "Element", new XAttribute("Type", "SqlTypeSpecifier"),
                            //                         new XElement(ns + "Relationship", new XAttribute("Name", "Type"),
                            //                             new XElement(ns + "Entry",
                            //                                 new XElement(ns + "References", new XAttribute("Name", $"{method.GetParameters()[0].ParameterType.GetAttribute<SqlUserDefinedTypeAttribute>().Name}") //TODO: I need a name builder
                            //                                 )
                            //                             )
                            //                         )
                            //                     )
                            //                 )
                            //             )
                            //        )
                            //    )
                            //),
                            new XElement(ns + "Relationship", new XAttribute("Name", "ReturnType"),
                                new XElement(ns + "Entry",
                                    new XElement(ns + "Element", new XAttribute("Type", "SqlTypeSpecifier"),
                                         new XElement(ns + "Relationship", new XAttribute("Name", "Type"),
                                             new XElement(ns + "Entry",
                                                 new XElement(ns + "Element", new XAttribute("Type", "SqlTypeSpecifier"),
                                                     new XElement(ns + "Relationship", new XAttribute("Name", "Type"),
                                                         new XElement(ns + "Entry",
                                                             new XElement(ns + "References", new XAttribute("Name", $"{method.GetParameters()[0].ParameterType.GetAttribute<SqlUserDefinedTypeAttribute>().Name}") //TODO: I need a name builder
                                                             )
                                                         )
                                                     )
                                                 )
                                             )
                                         )
                                    )
                                )
                            ),
                            new XElement(ns + "Relationship", new XAttribute("Name", "Schema"),
                                new XElement(ns + "Entry",
                                        new XElement(ns + "References", new XAttribute("ExternalSource", "Name"), new XAttribute("Name", "[dbo]") //TODO: I need a name builder
                                    )
                                )
                            )
                        );
        return aggregates;
    }

    public string GetName(object input) =>
    input switch
    {
        ParameterInfo parameter => $"{GetName(parameter.Member.DeclaringType)}.[@{parameter.Name}]",
        Type type => type.GetAttributes<SqlUserDefinedAggregateAttribute>().FirstOrDefault()?.Name,
        _ => throw new NotSupportedException()
    };

    public XElement Parameters(IEnumerable<ParameterInfo> parameters) =>
        new XElement(ns + "Relationship", new XAttribute("Name", "Parameters"),
            from parameter in parameters
            select new XElement(ns + "Entry",
                new XElement(ns + "Element", new XAttribute("Type", "SqlSubroutineParameter"), new XAttribute("Name", GetName(parameter)), //TODO: I need a name builder
                    new XElement(ns + "Relationship", new XAttribute("Name", "Type"),
                        new XElement(ns + "Entry",
                            new XElement(ns + "Element", new XAttribute("Type", "SqlTypeSpecifier"),
                                new XElement(ns + "Relationship", new XAttribute("Name", "Type"),
                                    new XElement(ns + "Entry",
                                        new XElement(ns + "References", new XAttribute("Name", $"{parameter.ParameterType.GetAttribute<SqlUserDefinedTypeAttribute>().Name}") //TODO: I need a name builder
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            )
        );

    public IEnumerable<XElement> Functions(XNamespace ns, Assembly assembly, string realAssemblyName)
    {
        var functions = from functionClasses in assembly.GetTypes().Where(t => t.IsAbstract)
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
                            new XElement(ns + "Relationship", new XAttribute("Name", "Parameters"),
                                new XElement(ns + "Entry",
                                    new XElement(ns + "Element", new XAttribute("Type", "SqlSubroutineParameter"),
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
                            )
                        );
        /*
			<Relationship Name="Parameters">
				<Entry>
					<Element Type="SqlSubroutineParameter" Name="[dbo].[Vector.Angle].[@vector1]">
						<Relationship Name="Type">
							<Entry>
								<Element Type="SqlTypeSpecifier">
									<Relationship Name="Type">
										<Entry>
											<References Name="[dbo].[Vector]" />
										</Entry>
									</Relationship>
								</Element>
							</Entry>
						</Relationship>
					</Element>
				</Entry>
				<Entry>
					<Element Type="SqlSubroutineParameter" Name="[dbo].[Vector.Angle].[@vector2]">
						<Relationship Name="Type">
							<Entry>
								<Element Type="SqlTypeSpecifier">
									<Relationship Name="Type">
										<Entry>
											<References Name="[dbo].[Vector]" />
										</Entry>
									</Relationship>
								</Element>
							</Entry>
						</Relationship>
					</Element>
				</Entry>
			</Relationship>
			<Relationship Name="Schema">
				<Entry>
					<References ExternalSource="BuiltIns" Name="[dbo]" />
				</Entry>
			</Relationship>
			<Relationship Name="Type">
				<Entry>
					<Element Type="SqlTypeSpecifier">
						<Property Name="Precision" Value="53" />
						<Relationship Name="Type">
							<Entry>
								<References ExternalSource="BuiltIns" Name="[float]" />
							</Entry>
						</Relationship>
					</Element>
				</Entry>
			</Relationship>
		</Element>
         */
        yield break;
    }
}
