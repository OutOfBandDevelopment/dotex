// Ignore Spelling: Dac

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace OoBDev.DacFx;

/// <summary>
/// Provides functionality for building DacPac (Data-tier Application Package) files from SQL CLR assemblies.
/// </summary>
/// <remarks>
/// <para>
/// A DacPac is a SQL Server deployment package that contains the schema and metadata for a database application.
/// This interface defines methods for extracting SQL CLR objects (functions, aggregates, user-defined types) from
/// .NET assemblies and packaging them into a deployable DacPac file.
/// </para>
/// <para>
/// The builder handles the complete lifecycle of DacPac creation including metadata generation, content hashing,
/// and XML model construction following the SQL Server DacFx specifications.
/// </para>
/// </remarks>
public interface IDacPacBuilder
{
    /// <summary>
    /// Extracts SQL CLR aggregate function definitions from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly containing SQL CLR aggregate definitions.</param>
    /// <param name="realAssemblyName">The actual assembly name to use in the output.</param>
    /// <returns>A collection of <see cref="XElement"/> objects representing aggregate definitions.</returns>
    IEnumerable<XElement> Aggregates(Assembly assembly, string realAssemblyName);

    /// <summary>
    /// Builds the content type metadata element for the DacPac package.
    /// </summary>
    /// <returns>An <see cref="XElement"/> representing the content type metadata.</returns>
    XElement BuildContentType();

    /// <summary>
    /// Builds the DacPac metadata element containing project information and version.
    /// </summary>
    /// <param name="projectName">The name of the database project.</param>
    /// <param name="versionNumber">The version number of the database project.</param>
    /// <returns>An <see cref="XElement"/> representing the DacPac metadata.</returns>
    XElement BuildDacMetadata(string projectName, string versionNumber);

    /// <summary>
    /// Builds a complete DacPac file from the specified assembly and optional PDB file.
    /// </summary>
    /// <param name="assemblyFileFramework">The path to the .NET assembly file containing SQL CLR objects.</param>
    /// <param name="assemblyPdbFramework">Optional path to the assembly's PDB (Program Database) file for debugging symbols.</param>
    /// <param name="dacpacFile">Optional output path for the DacPac file. If not specified, uses a default path.</param>
    /// <param name="projectName">Optional project name. If not specified, derives from assembly name.</param>
    /// <param name="projectVersion">Optional project version. If not specified, uses assembly version.</param>
    void BuildDacPac(string assemblyFileFramework, string? assemblyPdbFramework = null, string? dacpacFile = null, string? projectName = null, string? projectVersion = null);

    /// <summary>
    /// Builds the database model element from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly containing SQL CLR objects.</param>
    /// <param name="assemblyFile">The path to the assembly file.</param>
    /// <param name="pdbFile">Optional path to the PDB file.</param>
    /// <returns>An <see cref="XElement"/> representing the complete database model.</returns>
    XElement BuildModel(Assembly assembly, string assemblyFile, string? pdbFile);

    /// <summary>
    /// Builds the origin metadata element containing the model hash.
    /// </summary>
    /// <param name="modelHash">The SHA-256 hash of the database model.</param>
    /// <returns>An <see cref="XElement"/> representing the origin metadata.</returns>
    XElement BuildOrigin(string modelHash);

    /// <summary>
    /// Generates file entry elements for the assembly and optional PDB file.
    /// </summary>
    /// <param name="realAssemblyName">The actual assembly name to use in the output.</param>
    /// <param name="assemblyFile">The path to the assembly file.</param>
    /// <param name="pdbFile">Optional path to the PDB file.</param>
    /// <returns>A collection of <see cref="XElement"/> objects representing file entries in the package.</returns>
    IEnumerable<XElement> Files(string realAssemblyName, string assemblyFile, string? pdbFile);

    /// <summary>
    /// Builds parameter definition elements for a SQL CLR function.
    /// </summary>
    /// <param name="parameters">The function's parameter information.</param>
    /// <returns>An <see cref="XElement"/> representing the function parameters.</returns>
    XElement FunctionParameters(IEnumerable<ParameterInfo> parameters);

    /// <summary>
    /// Extracts SQL CLR function definitions from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly containing SQL CLR functions.</param>
    /// <param name="realAssemblyName">The actual assembly name to use in the output.</param>
    /// <returns>A collection of <see cref="XElement"/> objects representing function definitions.</returns>
    IEnumerable<XElement> Functions(Assembly assembly, string realAssemblyName);

    /// <summary>
    /// Reads the content of a file and returns it as a hexadecimal string.
    /// </summary>
    /// <param name="file">The path to the file to read.</param>
    /// <returns>A hexadecimal string representation of the file content, or <c>null</c> if the file cannot be read.</returns>
    string? GetHexContent(string file);

    /// <summary>
    /// Extracts the name from the specified object.
    /// </summary>
    /// <param name="input">The object to extract a name from (typically a reflection object like <see cref="MemberInfo"/>).</param>
    /// <returns>The extracted name, or <c>null</c> if the name cannot be determined.</returns>
    string? GetName(object? input);

    /// <summary>
    /// Computes the SHA-256 hash of the specified byte content.
    /// </summary>
    /// <param name="content">The byte array to hash.</param>
    /// <returns>A hexadecimal string representation of the SHA-256 hash.</returns>
    string GetSha256(byte[] content);

    /// <summary>
    /// Computes the SHA-256 hash of the specified file's content.
    /// </summary>
    /// <param name="file">The path to the file to hash.</param>
    /// <returns>A hexadecimal string representation of the SHA-256 hash.</returns>
    string GetSha256(string file);

    /// <summary>
    /// Computes the SHA-512 hash of the specified byte content.
    /// </summary>
    /// <param name="content">The byte array to hash.</param>
    /// <returns>A hexadecimal string representation of the SHA-512 hash.</returns>
    string GetSha512(byte[] content);

    /// <summary>
    /// Computes the SHA-512 hash of the specified file's content.
    /// </summary>
    /// <param name="file">The path to the file to hash.</param>
    /// <returns>A hexadecimal string representation of the SHA-512 hash.</returns>
    string GetSha512(string file);

    /// <summary>
    /// Builds parameter definition elements for a SQL CLR method.
    /// </summary>
    /// <param name="parameters">The method's parameter information.</param>
    /// <returns>
    /// An <see cref="XElement"/> representing the method parameters, or <c>null</c> if there are no parameters.
    /// </returns>
    XElement? MethodParameters(IEnumerable<ParameterInfo> parameters);

    /// <summary>
    /// Builds method definition elements for SQL CLR methods in the specified type.
    /// </summary>
    /// <param name="assembly">The assembly containing the SQL CLR type.</param>
    /// <param name="realAssemblyName">The actual assembly name to use in the output.</param>
    /// <param name="sqlClrType">The type containing SQL CLR methods.</param>
    /// <returns>An <see cref="XElement"/> representing the method definitions.</returns>
    XElement Methods(Assembly assembly, string realAssemblyName, Type sqlClrType);

    /// <summary>
    /// Builds property elements for the specified parameter.
    /// </summary>
    /// <param name="parameterInfo">The parameter information.</param>
    /// <returns>A collection of <see cref="XElement"/> objects representing parameter properties.</returns>
    IEnumerable<XElement> Properties(ParameterInfo parameterInfo);

    /// <summary>
    /// Builds a return type definition element for a SQL CLR function or method.
    /// </summary>
    /// <param name="returnInfo">The return parameter information.</param>
    /// <param name="isFunction">Indicates whether this is for a function (<c>true</c>) or method (<c>false</c>).</param>
    /// <returns>An <see cref="XElement"/> representing the return type definition.</returns>
    XElement Return(ParameterInfo returnInfo, bool isFunction);

    /// <summary>
    /// Builds a schema definition element from the specified input.
    /// </summary>
    /// <param name="input">The object containing schema information.</param>
    /// <returns>An <see cref="XElement"/> representing the schema definition.</returns>
    XElement Schema(object input);

    /// <summary>
    /// Builds a type specifier element for the specified parameter.
    /// </summary>
    /// <param name="parameterInfo">The parameter information.</param>
    /// <returns>An <see cref="XElement"/> representing the type specification.</returns>
    XElement TypeSpecifier(ParameterInfo parameterInfo);

    /// <summary>
    /// Extracts user-defined type (UDT) definitions from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly containing SQL CLR user-defined types.</param>
    /// <param name="realAssemblyName">The actual assembly name to use in the output.</param>
    /// <returns>A collection of <see cref="XElement"/> objects representing user-defined type definitions.</returns>
    IEnumerable<XElement> UserDefinedTypes(Assembly assembly, string realAssemblyName);
}
