// Ignore Spelling: Dac

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace OoBDev.DacFx;
public interface IDacPacBuilder
{
    IEnumerable<XElement> Aggregates(Assembly assembly, string realAssemblyName);
    XElement BuildContentType();
    XElement BuildDacMetadata(string projectName, string versionNumber);
    void BuildDacPac(string assemblyFileFramework, string assemblyFileNet, string? assemblyPdbFramework = null, string? dacpacFile = null, string? projectName = null, string? projectVersion = null);
    XElement BuildModel(Assembly assembly, string assemblyFile, string? pdbFile);
    XElement BuildOrigin(string modelHash);
    IEnumerable<XElement> Files(string realAssemblyName, string assemblyFile, string? pdbFile);
    XElement FunctionParameters(IEnumerable<ParameterInfo> parameters);
    IEnumerable<XElement> Functions(Assembly assembly, string realAssemblyName);
    string? GetHexContext(string file);
    string? GetName(object? input);
    string GetSha256(byte[] content);
    string GetSha256(string file);
    string GetSha512(byte[] content);
    string GetSha512(string file);
    XElement? MethodParameters(IEnumerable<ParameterInfo> parameters);
    XElement Methods(Assembly assembly, string realAssemblyName, Type sqlClrType);
    IEnumerable<XElement> Properties(ParameterInfo parameterInfo);
    XElement Return(ParameterInfo returnInfo, bool isFunction);
    XElement Schema(object input);
    XElement TypeSpecifier(ParameterInfo parameterInfo);
    IEnumerable<XElement> UserDefinedTypes(Assembly assembly, string realAssemblyName);
}