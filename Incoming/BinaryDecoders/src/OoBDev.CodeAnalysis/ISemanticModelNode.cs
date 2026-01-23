using Microsoft.CodeAnalysis;

namespace OoBDev.CodeAnalysis;

internal interface ISemanticModelNode
{
    SemanticModel Semantic { get; }
    object Node { get; }
}
