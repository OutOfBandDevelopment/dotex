namespace OoBDev.SemanticKernel;

/// <summary>
/// Marker interface for Semantic Kernel plugins that can be registered with dependency injection.
/// </summary>
/// <remarks>
/// This interface serves as a marker to identify classes that provide Semantic Kernel plugin functionality.
/// Implementations of this interface can be registered using the <see cref="ServiceCollectionExtensions.AddKernelPlugIn{T}"/>
/// extension method for automatic discovery and registration.
/// </remarks>
public interface IKernelPlugIn
{
}
