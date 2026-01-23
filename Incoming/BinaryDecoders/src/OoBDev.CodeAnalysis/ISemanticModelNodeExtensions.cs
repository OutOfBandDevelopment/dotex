using Microsoft.CodeAnalysis;

namespace OoBDev.CodeAnalysis;

internal static class ISemanticModelNodeExtensions
{
    public static ISemanticModelNode AddTo(this object obj, SemanticModel semantic) => new SemanticModelNode(semantic, obj);
    public static object WrapWith(this object obj, SemanticModel semantic) => obj.AddTo(semantic);
}
