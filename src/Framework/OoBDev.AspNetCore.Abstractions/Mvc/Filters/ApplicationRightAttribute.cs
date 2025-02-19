using Microsoft.AspNetCore.Mvc;

namespace OoBDev.AspNetCore.Mvc.Filters;

/// <summary>
/// At least one of these declared rights must be assigned to the user to access this point
/// </summary>
public class ApplicationRightAttribute : TypeFilterAttribute<ApplicationRightRequirementFilter>
{
    /// <summary>
    /// list of required rights
    /// </summary>
    public string[] Rights { get; }

    /// <summary>
    /// Declare required rights for endpoint
    /// </summary>
    /// <param name="rights"></param>
    public ApplicationRightAttribute(params string[] rights)
    {
        Rights = rights;
        Arguments = new[] { rights };
    }
}
