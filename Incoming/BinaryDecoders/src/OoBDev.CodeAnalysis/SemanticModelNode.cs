using Microsoft.CodeAnalysis;

namespace OoBDev.CodeAnalysis;

internal sealed class SemanticModelNode(SemanticModel semantic, object node) : ISemanticModelNode
{
    public SemanticModel Semantic { get; } = semantic;
    public object Node { get; } = node;
}
