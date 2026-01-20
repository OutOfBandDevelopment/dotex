using System.Reflection;
using System.Text.RegularExpressions;

namespace OoBDev.System.Utilities;

public class StringFormatter : IStringFormatter
{   
    public string? Format(string keyFormatter, MethodInfo method, object[] args)
    {
        if (string.IsNullOrWhiteSpace(keyFormatter))
            return null;

        var parameters = method.GetParameters();
        var result = keyFormatter;

        // Replace {paramName} patterns with actual values
        for (int i = 0; i < parameters.Length && i < args.Length; i++)
        {
            var param = parameters[i];
            var paramName = param.Name;
            var paramValue = args[i];

            // Handle simple parameter replacement: {param1} -> value
            var simplePattern = $"{{{paramName}}}";
            if (result.Contains(simplePattern))
            {
                result = result.Replace(simplePattern, paramValue?.ToString() ?? string.Empty);
            }

            // Handle property access: {model.Property} -> value
            var propertyPattern = new Regex($@"{{\s*{Regex.Escape(paramName)}\.(\w+)\s*}}");
            var matches = propertyPattern.Matches(result);
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    var propertyName = match.Groups[1].Value;
                    var propertyValue = paramValue?.GetType().GetProperty(propertyName)?.GetValue(paramValue);
                    result = result.Replace(match.Value, propertyValue?.ToString() ?? string.Empty);
                }
            }
        }

        return result;
    }
}
