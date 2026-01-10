using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Handlers;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Conversion
{
    public class DocumentConversionChainProvider : IDocumentConversionHandler
    {
        private readonly IDocumentConversionInputProvider _provider;
        private readonly ILogger<DocumentConversionChainProvider> _log;

        public DocumentConversionChainProvider(
            IDocumentConversionInputProvider provider,
            ILogger<DocumentConversionChainProvider> log
            )
        {
            _provider = provider;
            _log = log;
        }

        public async Task<byte[]?> ConvertAsync(DocumentTypes inputType, byte[] input, DocumentTypes outputType)
        {
            var chain = BuildChain(inputType, outputType).ToArray();

            if (!chain.Any())
            {
                _log.LogError($"No chain found from \"{inputType}\" to \"{outputType}\"");
                throw new UnhandledConversionRequestedException(inputType, outputType);
            }

            _log.LogInformation($"Start chain from \"{inputType}\" to \"{outputType}\"");
            var temp = input;
            foreach (var link in chain)
            {
                _log.LogInformation($"\tLinking \"{link.inputType}\" to \"{link.outputType}\"");
                if (temp != null)
                    temp = await link.handler.ConvertAsync(link.inputType, temp, link.outputType);
            }
            return temp;
        }

        private IEnumerable<(IDocumentConversionHandler handler, DocumentTypes inputType, DocumentTypes outputType)> BuildChain(DocumentTypes inputType, DocumentTypes outputType)
        {
            var possibleHandlers = _provider.GetInputHandlers(inputType);

            foreach (var handler in possibleHandlers)
            {
                if (handler.output == outputType)
                {
                    yield return (handler.handler, inputType, handler.output);
                    yield break;
                }
                else
                {
                    var children = BuildChain(handler.output, outputType);
                    foreach (var child in children)
                    {
                        yield return (handler.handler, inputType, handler.output);
                        yield return (child.handler, handler.output, child.outputType);
                        yield break;
                    }
                }
            }
        }
    }
}
