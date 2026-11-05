using System.Linq;

namespace OoBDev.Scpi.Panels;

/// <summary>The control ids a generated panel uses, so a caller can find a command's fields and reply.</summary>
public static class ScpiPanelIds
{
    /// <summary>The id of a parameter field: <c>{command}_{parameter}</c> with anything but letters and digits replaced by <c>_</c>.</summary>
    /// <param name="commandId">The command id.</param>
    /// <param name="parameterName">The parameter name.</param>
    public static string Field(string commandId, string parameterName) => Safe(commandId) + "_" + Safe(parameterName);

    /// <summary>The id of a query's reply field: <c>{command}_reply</c>.</summary>
    /// <param name="commandId">The command id.</param>
    public static string Reply(string commandId) => Safe(commandId) + "_reply";

    /// <summary>The id of a command's button, which is also the model's command property.</summary>
    /// <param name="commandId">The command id.</param>
    public static string Button(string commandId) => Safe(commandId);

    private static string Safe(string text) => new([.. text.Select(c => char.IsLetterOrDigit(c) ? c : '_')]);
}
