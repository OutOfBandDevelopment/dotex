using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Documents.Conversion;

/// <summary>
/// Represents a builder for constructing document conversion chains.
/// </summary>
public class DocumentConversionChainBuilder : IDocumentConversionChainBuilder
{
    private readonly IEnumerable<IDocumentConversionHandler> _handlers;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentConversionChainBuilder"/> class.
    /// </summary>
    /// <param name="handlers">The available document conversion handlers.</param>
    public DocumentConversionChainBuilder(IEnumerable<IDocumentConversionHandler> handlers) => _handlers = handlers;

    /// <summary>
    /// Constructs the document conversion steps for converting from the specified source content type to the destination content type.
    /// </summary>
    /// <param name="sourceContentType">The content type of the source document.</param>
    /// <param name="destinationContentType">The desired content type of the converted document.</param>
    /// <returns>An array of ChainStep objects representing the conversion steps.</returns>
    public virtual ChainStep[] Steps(string sourceContentType, string destinationContentType)
    {
        var simple = _handlers
            .FirstOrDefault(h => h.SupportedSource(sourceContentType) && h.SupportedDestination(destinationContentType));
        if (simple != null) return [new ChainStep() { Handler = simple, SourceContentType = sourceContentType, DestinationContentType = destinationContentType }];

        var chains = Chains(sourceContentType, destinationContentType, []).ToArray();
        var selected = chains.OrderByDescending(c => c.Length).FirstOrDefault();
        return selected ?? [];
    }

    private IEnumerable<ChainStep[]> Chains(
        string sourceContentType,
        string destinationContentType,
        List<ChainStep> parents)
    {
        var realPossibles = _handlers
            .Except(parents.Select(p => p.Handler))
            .Where(i => i.SupportedSource(sourceContentType))
            .ToArray();
        if (realPossibles.Length == 0) yield break;

        var ends = realPossibles.Where(d => d.SupportedDestination(destinationContentType)).ToArray();
        if (ends.Length != 0)
        {
            foreach (var tail in ends)
            {
                parents.Add(new ChainStep
                {
                    Handler = tail,
                    SourceContentType = sourceContentType,
                    DestinationContentType = destinationContentType
                });
                yield return parents.ToArray();
            }
        }
        else
        {
            foreach (var maybe in realPossibles)
            {
                foreach (var maybeSourceNext in maybe.Destinations)
                {
                    var newParents = new List<ChainStep>(parents)
                    {
                        new() {
                            Handler = maybe,
                            SourceContentType = sourceContentType,
                            DestinationContentType = maybeSourceNext
                        }
                    };
                    var nextChildren = Chains(maybeSourceNext, destinationContentType, newParents).ToArray();
                    foreach (var next in nextChildren)
                    {
                        if (next == null || next.Length == 0) continue;
                        yield return next;
                    }
                }
            }
        }
    }
}

