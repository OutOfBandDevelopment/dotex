using System.Reflection;
using System.Text.RegularExpressions;

namespace OoBDev.System.Utilities;

public class StringFormatter : IStringFormatter
{
    public string? Format(string keyFormatter, MethodInfo method, object?[]? args)
    {
        if (string.IsNullOrWhiteSpace(keyFormatter))
            return null;

        var parameters = method.GetParameters();
        var result = keyFormatter;

        // Replace {paramName} or {paramName.Property.Chain} patterns with actual values
        if (args != null)
            for (var i = 0; i < parameters.Length && i < args.Length; i++)
            {
                var param = parameters[i];
                var paramName = param.Name ?? $"[{i}]";
                var paramValue = args[i];

                // Handle property chains: {model.Property.SubProperty} -> value
                // This regex captures: {paramName.prop1.prop2.prop3}
                var propertyChainPattern = new Regex($@"{{\s*{Regex.Escape(paramName)}((?:\.\w+)+)\s*}}");
                var matches = propertyChainPattern.Matches(result);
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        var propertyChain = match.Groups[1].Value.TrimStart('.'); // Remove leading dot
                        var propertyValue = GetPropertyChainValue(paramValue, propertyChain);
                        result = result.Replace(match.Value, propertyValue?.ToString() ?? string.Empty);
                    }
                }

                // Handle simple parameter replacement: {param1} -> value
                // This must come after property chain replacement to avoid replacing parts of property chains
                var simplePattern = $"{{{paramName}}}";
                if (result.Contains(simplePattern))
                {
                    result = result.Replace(simplePattern, paramValue?.ToString() ?? string.Empty);
                }
            }

        return result;
    }

    /// <summary>
    /// Resolves a property chain like "Name" or "Address.City" or "User.Address.City"
    /// </summary>
    private static object? GetPropertyChainValue(object? obj, string propertyChain)
    {
        if (obj == null || string.IsNullOrWhiteSpace(propertyChain))
            return null;

        var properties = propertyChain.Split('.');
        object? current = obj;

        foreach (var propertyName in properties)
        {
            if (current == null)
                return null;

            var property = current.GetType().GetProperty(propertyName);
            if (property == null)
                return null;

            current = property.GetValue(current);
        }

        return current;
    }
}
