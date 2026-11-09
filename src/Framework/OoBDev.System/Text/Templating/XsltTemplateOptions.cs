using System.Collections.Generic;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Options of <see cref="XsltTemplateProvider"/>.
/// </summary>
public class XsltTemplateOptions
{
    /// <summary>
    /// Gets the extension objects whose public methods become XSLT functions (see <c>XsltArgumentListExtensions</c>),
    /// for example the path, file and string extensions. Empty by default, so a template has no extra functions.
    /// </summary>
    public IList<object> Extensions { get; } = [];
}
