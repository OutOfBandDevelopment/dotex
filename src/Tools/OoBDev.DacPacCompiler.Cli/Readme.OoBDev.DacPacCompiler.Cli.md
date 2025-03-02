# Lightwell Shared Framework - DACPAC Compiler

## Summary

This tool is used to merge multiple DACPAC files into a single package.

## Command line parameters

* -t | --template: Use this value to provide a path to a configuration template yaml file.
* -s | --source-path: base directory to be used with source patterns
* -p | --source-patterns: File globbing pattern to select dacpacs to be merged
* -r | --target-path: Full output path for merged dacpac results
* -d | --description: Description to be used added to the dacpac metadata
* -n | --name: Name to be used added to the dacpac metadata.  If not provided the name of the output file will be used.
* -v | --version: version number to apple to dacpac metadata
* -b | --build-version: extended version number to be provided to the build.  if not provided the version will be applied here as well

## Template File Example

```yml
SourcePath: "."
SourcePatterns:
- "*.dacpac"

TargetPath: composed_XXXXXXXXXX.dacpac
ServerVersion: SqlAzure # 

ModelOptionSource: Custom
ModelOptions: # this is based on Microsoft.SqlServer.Dac.Model.TSqlModelOptions, Microsoft.SqlServer.Dac.Extensions
- Property: Value

Name: test name
Description: test desc
Version: 1.2.3
BuildVersion: 20200527-1.2.3-XYZ.4

```