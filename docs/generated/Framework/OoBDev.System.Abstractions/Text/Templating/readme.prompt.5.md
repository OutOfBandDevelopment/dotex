Create a readme file describing the general functionality over the follow files.

Generate a summary that includes a description of the related functionality.
In a technical summary define any design patterns or achitectural patterns described by the files.
Generate a component diagrams using plantuml.
PlantUML blocks must all be identified with "```plantuml" and closed with "```"

## Source Files

```ITemplateProvider.cs
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Represents a template provider that can apply templates based on a specified context.
/// </summary>
public interface ITemplateProvider
{
    /// <summary>
    /// Gets the collection of supported content types by the template provider.
    /// </summary>
    IReadOnlyCollection<string> SupportedContentTypes { get; }

    /// <summary>
    /// Determines whether the template provider can apply a template based on the provided context.
    /// </summary>
    /// <param name="context">The template context.</param>
    /// <returns><c>true</c> if the template provider can apply the template; otherwise, <c>false</c>.</returns>
    bool CanApply(ITemplateContext context);

    /// <summary>
    /// Applies the template associated with the specified context, using the provided data, and writes the result to the target stream asynchronously.
    /// </summary>
    /// <param name="context">The template context.</param>
    /// <param name="data">The data to apply to the template.</param>
    /// <param name="target">The stream where the result will be written.</param>
    /// <returns>A task representing the asynchronous operation, indicating whether the application was successful.</returns>
    Task<bool> ApplyAsync(ITemplateContext context, object data, Stream target);
}

```

```ITemplateSource.cs
using System.Collections.Generic;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Represents a source of templates that can be used by a template engine.
/// </summary>
public interface ITemplateSource
{
    /// <summary>
    /// Gets the template contexts associated with the specified template name.
    /// </summary>
    /// <param name="templateName">The name of the template to retrieve.</param>
    /// <returns>An enumerable collection of template contexts.</returns>
    IEnumerable<ITemplateContext> Get(string templateName);
}

```

