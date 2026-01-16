using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace OoBDev.DocumentCenter.Conversion
{
    public class DocumentConversionHandlerResolver : IDocumentConversionHandlerResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDocumentConversionInputProvider _provider;
        public DocumentConversionHandlerResolver(
            IServiceProvider serviceProvider,
            IDocumentConversionInputProvider provider
            )
        {
            _serviceProvider = serviceProvider;
            _provider = provider;
        }

        public IDocumentConversionHandler GetHandler(DocumentTypes inputType, DocumentTypes outputType)
        {
            var inputHandlers = _provider.GetInputHandlers(inputType);
            var (handler, _) = inputHandlers.FirstOrDefault(h => h.output == outputType);
            return handler ??  ActivatorUtilities.CreateInstance<DocumentConversionChainProvider>(_serviceProvider);
        }
    }
}
