using OoBDev.System.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OoBDev.Extensions.Configuration;

/// <summary>
/// builder pattern for command parameter arguments
/// </summary>
public static class CommandLine
{
    private readonly static StringComparer Comparer = StringComparer.InvariantCultureIgnoreCase;

    /// <summary>
    /// add additional configurable parameters
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IDictionary<string, string> AddParameters<T>(this IDictionary<string, string> items) =>
        items.Concat(BuildParameters<T>())
             .GroupBy(i => i.Key, Comparer)
             .ToDictionary(i => i.Key, i => i.First().Value, Comparer)
             ;

    /// <summary>True when the arguments ask for usage text (<c>-h</c>, <c>--help</c> or <c>-?</c>).</summary>
    /// <param name="args">Command-line arguments.</param>
    public static bool IsHelpRequest(IEnumerable<string> args) =>
        args.Any(a => a is "-h" or "--help" or "-?");

    /// <summary>
    /// Builds the usage text of a tool from the <see cref="CommandParameterAttribute"/> and
    /// <see cref="DescriptionAttribute"/> of the properties of its option types; the same types always give the same text.
    /// </summary>
    /// <param name="toolName">Name shown in the first line.</param>
    /// <param name="summary">One sentence about what the tool does.</param>
    /// <param name="optionTypes">Option classes bound from the command line, in display order.</param>
    public static string Usage(string toolName, string summary, params Type[] optionTypes)
    {
        var lines = new List<(string Switch, string Detail)>();
        foreach (var type in optionTypes)
        {
            foreach (var property in TypeDescriptor.GetProperties(type).OfType<PropertyDescriptor>())
            {
                var attribute = property.Attributes.OfType<CommandParameterAttribute>().FirstOrDefault();
                var names = new[] { attribute?.Short, attribute?.Value ?? property.Name }
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(n => $"--{n}");
                var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                var kind = propertyType.IsEnum
                    ? string.Join('|', Enum.GetNames(propertyType))
                    : propertyType == typeof(bool) ? "true|false" : propertyType.Name.ToLowerInvariant();
                var required = property.Attributes.OfType<global::System.ComponentModel.DataAnnotations.RequiredAttribute>().Any()
                    || type.GetProperty(property.Name)?.GetCustomAttributes(typeof(global::System.Runtime.CompilerServices.RequiredMemberAttribute), true).Length > 0;
                var description = property.Attributes.OfType<DescriptionAttribute>().FirstOrDefault()?.Description;
                var detail = string.Join(' ', new[] { $"<{kind}>", required ? "(required)" : null, description }.Where(p => !string.IsNullOrEmpty(p)));
                lines.Add((string.Join(", ", names), detail));
            }
        }

        var width = lines.Count == 0 ? 0 : lines.Max(l => l.Switch.Length);
        var text = new global::System.Text.StringBuilder();
        text.Append(toolName).Append(" - ").Append(summary).Append('\n');
        text.Append("Pass options as --name value (a boolean needs an explicit true or false).\n");
        foreach (var (switchText, detail) in lines)
        {
            text.Append("  ").Append(switchText.PadRight(width)).Append("  ").Append(detail).Append('\n');
        }

        return text.ToString();
    }

    /// <summary>
    /// entry point or defining configurable parameters
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IDictionary<string, string> BuildParameters<T>()
    {
        var type = typeof(T);

        var properties = from property in TypeDescriptor.GetProperties(type).OfType<PropertyDescriptor>()
                         let attribute = property.Attributes.OfType<CommandParameterAttribute>().FirstOrDefault()
                         from parameter in new[] { attribute?.Short, attribute?.Value, property.Name }
                         where !string.IsNullOrWhiteSpace(parameter)
                         select new
                         {
                             Key = $"--{parameter}",
                             Value = $"{type.Name}:{property.Name}"
                         };
        var dictionary =
            properties.GroupBy(i => i.Key, Comparer)
                      .ToDictionary(i => i.Key, i => i.First().Value, Comparer)
                      ;
        return dictionary;
    }
}
