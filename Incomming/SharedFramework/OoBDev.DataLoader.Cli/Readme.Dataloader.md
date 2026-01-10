# OoBDev Shared Framework - Data Loader

## Summary

This is tool may be used to upload content into a Entity Framework based dotnet solution

## Command line parameters

* -t | --template: Use this value to provide a path to a configuration template yaml file.
* -b | --base-path: base directory.  If set system will ensure other directories are relative children to this path.
* -c | --connection-string: database connection string to be provided to the DBContextFactory
* -x | --contexts: List of DB Contexts to be used 
* -a | --assembly-path: path that contains the .Net assemblies related to the defined DBContexts
* -d | --data-paths: folders containing related content to upload
* --setting-masterdata: First|Skip|Last
* --setting-duplicate-handling: Fail|First|Last
* --setting-disable-fk: true|false

## Template File Example

```yml
ConnectionString: server=(localdb)\ApplicationCore;database=ApplicationDb
BasePath: C:\Repos\Application\Core
AssemblySearchPath: .\ApiServices\src\WebApi\bin\Debug\netcoreapp3.1
UserSession:
  UserId: DDE27BBD-5647-4611-B662-088C7FBA3E55
  Username: SystemUser
Contexts:
- OoBDev.Core.Persistence.CoreDbContext, OoBDev.Core.Persistence
- OoBDev.Core.AnnouncementBanners.Persistence.CoreBannersDbContext, OoBDev.Core.AnnouncementBanners
- OoBDev.Core.ClaimScopes.Persistence.CoreClaimScopesDbContext, OoBDev.Core.ClaimScopes
- OoBDev.Core.CMS.Persistence.CMSDbContext, OoBDev.Core.CMS
- OoBDev.Core.DynamicForms.Persistence.DynamicFormsDbContext, OoBDev.Core.DynamicForms
- OoBDev.Core.Languages.Persistence.CoreLanguagesDbContext, OoBDev.Core.Languages
SourceDataPaths:
- .\conf\dataLoading\config-base
- .\conf\dataLoading\config-demoSet1
- .\conf\dataLoading\transaction-demoSet1
DuplicateHandling: First # Fail|First|Last
ImportMasterData: Last # First|Skip|Last
DisableForeignKeysBeforeInsert: true  # true|false

```

## Supported Input Files Types

Source data may be in JSON or CSV format.  

### JSON

JSON should be an object array of a similar structural shape as the related entity object. 

### CSV

CSV file must contain a header records where the column names match the entity property names.
Records may not have line feeds and each field should be separated by a comma and wrapped with quotation marks.  
Blank lines will be ignored and lines may be commented with a # mark.


## Extension Examples

### Lookup existing data by Alternate Key

If source data is not defined (null or default) the system will try to resolve the primary key from the 
database by using any alternate key/values defined.  

### Upload Document Content to Blob Store

It is possible to upload file content to using the Shared Frameworks Blob Container.  

If you have fields on your imported file named `__SourceFilePath` the system will try to upload 
that file to the blob store. This path maybe be relative to the source data file.  If `__SourceFileContent` 
is provided that value will be used for the documents content type.  If not provided the content 
type will be added as `application/octet-stream`.  Once the file is uploaded the fields will be 
replaced by `StorageContainer` and `StorageKey` which are the key values returned from the blob 
store.

### Get Text Data

It is possible to retrieve text content externally from another file and replace one or more fields 
within your entity data from the file.  Adding a field to your document starting with `__GetText` 
will use relative path value in that property to replace the text content for the matched field.

If for example your field is named `__GetText_LongText` and contains the value `./SourceData/LongTextExample.txt`
The system will use the `SourceData` directory under the directory containing your source file 
to locate the file named `LongTextExample.txt` and if that file exists a new field named `LongText` 
replace the `__GetText_LongText` with the content of the matched file.

#### Example Entity Source
```json
[
	{
		"MyId": 1,
		"__GetText_Template" : "./SourceData/Template.txt"
	}
]
```

#### Example source file
```txt
Hello World!
```
#### Example Entity Result
```json
[
	{
		"MyId": 1,
		"Template" : "Hello World!"
	}
]
```

### Secondary Lookup

It is possible to do a secondary SQL lookup during the pipeline processing to enhance the 
required entity data.  This may be used for things like fixing foreign key relations.

If you add a field named `__Lookup_Query` and that field contains a valid SQL statement 
it will execute the query and replace the `__Lookup_Query` value with the fields return
from the SQL statement.

#### Example Entity Source

```json
[
	{
		"MyId": 1
		"__Lookup_Query": "SELECT TOP (1) [DocumentIdentity], [CreatedBy] AS [RelatedBy] FROM [Core].[Documents] WHERE [DocumentId] = 'ABC'"
	}
]
```

#### Example Data

| DocumentId | DocumentIdentity | CreatedBy |
| ---------- | ---------------- | --------- |
| ABC        | 321              | Matt      |

#### Example Entity Result

```json
[
	{
		"MyId": 1
		"DocumentIdentity": 321,
		"RelatedBy": "Matt"
	}
]
```