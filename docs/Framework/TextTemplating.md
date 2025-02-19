# OoBDev - Text Templating

See [back](MajorFunctionality.md)

## Summary

The text templating engine exists to provide a centralized way to transform object model text.

The engine is extensible to support multiple template providers such as 
[Handlebars](https://handlebarsjs.com/guide/) and [XSLT](https://www.w3schools.com/xml/xsl_intro.asp).


Text Templating is a native portion of the framework.  It is designed to provide a common way to 
generate simple text content. `OoBDev.System.Text.Templating`

#### Current Implementations

* XSLT 1.0 - Built in `XsltTemplateProvider`
* Handlebars.Net - Requires `OoBDev.Handlebars`

## Related Notes

* [System](../Libraries/OoBDev.System.md)
  * [System.Abstractions](../Libraries/OoBDev.System.Abstractions.md)
* [Handlebars](../Libraries/OoBDev.Handlebars.md)
  * [Notes](../code/OoBDev.Handlebars/Readme.Handlebars.md)

## Structure

```plantuml

package Abstractions {
    interface "ILogger<>" as ILogger

    interface ITemplateEngine {
        + ApplyAsync(string, object, Stream) : Task<ITemplateContext>
        + ApplyAsync(ITemplateContext, object, Stream) : Task<bool>
        + ApplyAsync(string, object) : Task<string>
        + ApplyAsync(ITemplateContext, object) : Task<string>
        + Get(string) : ITemplateContext
        + GetAll(string) : IEnumerable<ITemplateContext>
    }

    interface ITemplateSource {
        + Get(string) : ITemplateContext
    }

    interface ITemplateProvider {
        + SupportedContentTypes : string[]
        + CanApply(ITemplateContext) : bool
        + ApplyAsync(ITemplateContext, object, Stream) : Task<bool>
    }

    interface IFileType {
        + Extension      : string
        + ContentType    : string
        + IsTemplateType : bool
    }

    interface ITemplateContext {
        + TemplateName          : string
        + TemplateContentType   : string
        + TemplateFileExtension : string
        + TemplateSource        : ITemplateSource
        + TemplateReference     : string
        + TemplateFileExtension : string        
        + TargetContentType     : string
        + TargetFileExtension   : string        
        + Priority              : int
        + OpenTemplate          : Func<ITemplateContext, Stream>
    }
}

package System {
    class  TemplateEngine {
        - logger    : ILogger
        - sources   : ITemplateSource[]
        - providers : ITemplateProvider[]
    }

    class XsltTemplateProvider {
        + SupportedContentTypes = "application/xslt+xml"
    }

    class FileTemplateSource {
        - logger    : ILogger
        - fileTypes : IFileType[]
        - config    : FileTemplatingSettings 
    }
}

package Extensions {
    class HandlebarsTemplateProvider {
        - logger : ILogger
        + SupportedContentTypes = "text/x-handlebars-template"
    }
}

ITemplateEngine     --> ITemplateContext : uses
ITemplateSource     --> ITemplateContext : uses
ITemplateProvider   --> ITemplateContext : uses

ITemplateEngine     ^-- TemplateEngine : implements
TemplateEngine      --* ILogger : uses
TemplateEngine      --o ITemplateSource : uses
TemplateEngine      --o ITemplateProvider : uses

ITemplateProvider   ^-- HandlebarsTemplateProvider : implements
HandlebarsTemplateProvider  --* ILogger : uses

ITemplateProvider   ^-- XsltTemplateProvider : implements

ITemplateSource     ^-- FileTemplateSource : implements
FileTemplateSource  --* ILogger : uses
FileTemplateSource  --o IFileType : uses

```

## Features and Components 

### OoBDev.System.Text.Templating.ITemplateEngine

The template engine is the central component. This allows for resolving the matched templates 
by name and performing the related transformation

### OoBDev.System.Text.Templating.ITemplateSource

Template Sources are a means to lookup/resolve templates for your application. An example template 
source is  FileTemplateSource where the template is defined in a file on the local file system. 

### OoBDev.System.Text.Templating.ITemplateProvider

Template providers are an actual means to perform a transform. Providers could be something such
as XSLT or Handlebars.

### OoBDev.System.Text.Templating.IFileType

File types are a simple registration to help mapping between content types and file extensions as 
well as additional metadata such as "Is Template"

## Examples

### How to use the Template Engine

1. Ensure the Templating Extensions are registered with your application

```csharp
using OoBDev.System;
using OoBDev.Handlebars.Extensions;

// register OoBDev System Extensions
builder.Services.TryAddSystemExtensions(builder.Configuration);
// or just the templating extensions
//builder.Services.TryTemplatingExtensions(builder.Configuration);

// additional templating extensions may also be registered such as 
builder.Services.TryAddHandlebarServices();
``` 

2. Inject the `ITemplateEngine` into your component

The framework will look though the registered `ITemplateSource` for a template that matches the value 
for `templateName`.  Once the template is returned the `ITemplateProvider` will be selected based on 
the `TemplateContentType` from the context and the `data` object will be provided to provider to be 
used as a source for related mapping values

```csharp
public class YourComponent(
    ITemplateEngine templateEngine
) {
  public const string YourTemplate = "your-template-name";

  public async Task<string> DoWork(object data) => 
    await templateEngine.ApplyAsync(templateName: YourTemplate, data);
}
```

### Template sources

The built in template source will try to retreive the file from the local file system based on the 
configured base path and the template name as the file name.  The format of template content is 
dependant on the related provider.

---

See [back](MajorFunctionality.md)