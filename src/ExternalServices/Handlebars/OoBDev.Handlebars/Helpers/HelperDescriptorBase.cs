using HandlebarsDotNet;
using HandlebarsDotNet.Helpers;
using HandlebarsDotNet.PathStructure;

namespace OoBDev.Handlebars.Helpers;

/// <summary>
/// Base class for helper descriptors.
/// </summary>
public abstract class HelperDescriptorBase : IHelperDescriptor<HelperOptions>
{
    /// <summary>
    /// Gets the Handlebars helper associated with this descriptor.
    /// </summary>
    /// <remarks>
    /// This property should return the Handlebars helper implementation.
    /// </remarks>
    protected abstract HandlebarsHelper Helper { get; }

    /// <summary>
    /// Gets the name of the helper.
    /// </summary>
    public abstract PathInfo Name { get; }

    /// <summary>
    /// Invokes the helper with the specified options, context, and arguments.
    /// </summary>
    /// <param name="options">The options for the helper.</param>
    /// <param name="context">The context in which the helper is being executed.</param>
    /// <param name="arguments">The arguments passed to the helper.</param>
    /// <returns>The result of invoking the helper.</returns>
    public object Invoke(in HelperOptions options, in Context context, in Arguments arguments) =>
        this.ReturnInvoke(options, context, arguments);

    /// <summary>
    /// Invokes the helper and writes the result to the output.
    /// </summary>
    /// <param name="output">The output writer.</param>
    /// <param name="options">The options for the helper.</param>
    /// <param name="context">The context in which the helper is being executed.</param>
    /// <param name="arguments">The arguments passed to the helper.</param>
    public void Invoke(in EncodedTextWriter output, in HelperOptions options, in Context context, in Arguments arguments) =>
        Helper(output, context, arguments);
}
