# Document Center - Document Conversion

## Summary

As part of the Document Center we will have the ability to convert between various document types/formats.  

## Document Conversion

Using `IDocumentConverter` allows file content to be changed from their current document type to a new type.  This content may be provided directly or by reference key to a document already in the document store.  It is also possible to return the new content or automatically persist the content and return the reference.  

## Document Conversion Handler

Document Conversion Handlers are responsible for converting content based on selected input and output document types. 

To implement a Document Conversion Handler you must...

* Use the `IDocumentConversionHandler` interface in the `OoBDev.DocumentCenter.Contracts` assembly.  
* Add a `DocumentHandlerAttribute` to your class to define the handled document type input/output combinations.  (These are used by the runtime to resolve allowed operations.)  
* Register your class in the IOC container.  

### Example

#### Class Implementation 

 ```csharp
using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Handlers;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OoBDev.DocumentCenter.Contracts.DocumentTypes;

namespace OoBDev.Api.YourAssembly.Handlers
{
    [DocumentHandler(InputType = Text, OutputType = Html)]
    public class Text2HtmlDocumentConversionHandler : IDocumentConversionHandler
    {
        public Task<byte[]> ConvertAsync(DocumentTypes inputType, byte[] input, DocumentTypes outputType)
        {
            if (!new[] { Text }.Contains(inputType)) throw new InvalidConversionInputException(inputType);
            if (!new[] { Html }.Contains(outputType)) throw new InvalidConversionOutputException(outputType);
            if (input == null || input.Length == 0) return null;

            var text = Encoding.UTF8.GetString(input);
            var result = Encoding.UTF8.GetBytes($"<html><body><pre>{text}</pre></body></html>");
            return Task.FromResult(result);
        }
    }
}
 ```

#### IOC Registration

 ```csharp
using OoBDev.Api.YourAssembly.Handlers;
using OoBDev.DocumentCenter.Contracts.Handlers;
using OoBDev.Toolkit.Common;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Api.YourAssembly
{
    public class YourAssemblyRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services) =>
            services
                .AddTransient<IDocumentConversionHandler, Text2HtmlDocumentConversionHandler>()
                ;
    }
}
 ```

### Implicit Conversion

If no direct conversions are defined the system has a special handler called `DocumentConversionChainProvider`.  This handler will try to resolve a chain from the requested input type to the output type.  

#### Example

Assume the system has a `CSV to HTML` and an `HTML to PDF` but no `CSV to PDF`.  If you request the conversion of `CSV to PDF` this handler will build a conversion chain by first calling `CSV to HTML` then `HTML to PDF`.  

If no conversion chain is possible an `UnhandledConversionRequestedException` exception will be thrown.  
