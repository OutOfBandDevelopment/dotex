using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace OoBDev.DocumentCenter.Conversion
{
    public class DocumentConversionInputProvider : IDocumentConversionInputProvider
    {
        private readonly IEnumerable<IDocumentConversionHandler> _handlers;
        public DocumentConversionInputProvider(
            IEnumerable<IDocumentConversionHandler> handlers
            )
        {
            _handlers = handlers;
        }

        public IEnumerable<(IDocumentConversionHandler handler, DocumentTypes output)> GetInputHandlers(DocumentTypes inputType) =>
            from handler in _handlers
            from attribute in TypeDescriptor.GetAttributes(handler).OfType<DocumentHandlerAttribute>()
            where attribute.InputType == inputType
            orderby attribute.Priority descending
            select (handler, attribute.OutputType);
    }
}
