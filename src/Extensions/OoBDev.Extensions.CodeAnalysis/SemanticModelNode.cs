using Microsoft.CodeAnalysis;

namespace OoBDev.Extensions.CodeAnalysis;

/// <summary>Pairs a syntax or symbol item with the semantic model that resolves it.</summary>
internal sealed record SemanticModelNode(SemanticModel Semantic, object Node);
