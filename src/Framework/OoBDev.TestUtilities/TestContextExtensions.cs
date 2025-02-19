using OoBDev.Extensions.Reflection;
using OoBDev.System.Text.Json.Serialization;
using OoBDev.System.Text.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace OoBDev.TestUtilities;

/// <summary>
/// Extensions for TestContext 
/// </summary>
public static class TestContextExtensions
{
    /// <summary>
    /// serialize an object to the test results for a given test run
    /// </summary>
    /// <param name="context"></param>
    /// <param name="value"></param>
    /// <param name="caller"></param>
    /// <param name="callerLine"></param>
    /// <param name="callerFile"></param>
    /// <returns></returns>
    public static TestContext? AddResult(
        this TestContext? context,
        object? value,
        [CallerMemberName] string? caller = default,
        [CallerLineNumber] int callerLine = default,
        [CallerFilePath] string? callerFile = default
        ) =>
        context.AddResult(value, out var _, caller, callerLine, callerFile);

    /// <summary>
    /// serialize an object to the test results for a given test run
    /// </summary>
    public static TestContext? AddResult(
        this TestContext? context,
        object? value,
        out string? outFile,
        [CallerMemberName] string? caller = default,
        [CallerLineNumber] int callerLine = default,
        [CallerFilePath] string? callerFile = default
        ) =>
        context.AddResult(value, "", out outFile, caller, callerLine, callerFile);

    /// <summary>
    /// serialize an object to the test results for a given test run
    /// </summary>
    public static TestContext? AddResult(
        this TestContext? context,
        object? value,
        string fileName,
        [CallerMemberName] string? caller = default,
        [CallerLineNumber] int callerLine = default,
        [CallerFilePath] string? callerFile = default
        ) =>
        context.AddResult(value, fileName, out _, caller, callerLine, callerFile);

    /// <summary>
    /// serialize an object to the test results for a given test run
    /// </summary>
    public static TestContext? AddResult(
        this TestContext? context,
        object? value,
        string? fileName,
        out string? outFile,
        [CallerMemberName] string? caller = default,
        [CallerLineNumber] int callerLine = default,
        [CallerFilePath] string? callerFile = default
        )
    {
        if (context == null)
        {
            outFile = null;
            return default;
        }

        var baseFileName = string.IsNullOrWhiteSpace(fileName) ? value?.GetType()?.Name : Path.GetFileNameWithoutExtension(fileName);
        var baseFileExtension = string.IsNullOrWhiteSpace(fileName) ? "" : Path.GetExtension(fileName);
        var allowChangeExtension = false;
        var timeStamp = DateTime.Now.Ticks;

        if (string.IsNullOrWhiteSpace(baseFileExtension) && (!fileName?.EndsWith('.') ?? true))
        {
            baseFileExtension = ".json";
            allowChangeExtension = true;
        }

        string changeExtension(string inputFileName, string extension) => allowChangeExtension ? Path.ChangeExtension(inputFileName, extension) : inputFileName;
        string cleanFileName(string inputFileName) =>
            "`:<>+&|'\"\b\0\t\r\n"
                .Concat(Enumerable.Range(1, 30).Select(i => (char)i))
                .Concat(Path.GetInvalidPathChars())
                .Concat(Path.GetInvalidFileNameChars())
                .Distinct()
                .Aggregate(new StringBuilder(inputFileName), (sb, v) => sb.Replace(v, '_'), sb => sb.ToString());

        var composedFileName = cleanFileName(string.Join('-',
            baseFileName,
            $"{Path.GetFileNameWithoutExtension(callerFile)}_{caller}({callerLine})",
            timeStamp
            ) + baseFileExtension);

        if (value == null)
        {
            outFile = null;
            context.WriteLine($"{composedFileName}: No output");
            return context;
        }

        if (value is byte[] data)
        {
            var file = changeExtension(composedFileName, ".bin");
            AddResultFile(context, file, data, out outFile);
            context.WriteLine($"{file}: Attached");
        }
        else if (value is XFragment || value is XmlReader)
        {
            var xFragment = value as XFragment;
            xFragment ??= new XFragment((XmlReader)value);

            var file = changeExtension(composedFileName, ".xml");
            var ms = new MemoryStream();
            var writer = XmlWriter.Create(ms, new XmlWriterSettings
            {
                CloseOutput = false,
                ConformanceLevel = ConformanceLevel.Fragment,
                Indent = true,
                NewLineHandling = NewLineHandling.Replace,
                OmitXmlDeclaration = true,
            });
            foreach (var xNode in xFragment)
                xNode.WriteTo(writer);
            writer.Flush();
            AddResultFile(context, file, ms.ToArray(), out outFile);
            context.WriteLine($"{file}: Attached");
        }
        else if (value is XNode xNode)
        {
            var file = changeExtension(composedFileName, ".xml");
            var ms = new MemoryStream();
            var writer = XmlWriter.Create(ms, new XmlWriterSettings
            {
                CloseOutput = false,
                ConformanceLevel = ConformanceLevel.Fragment,
                Indent = true,
                NewLineHandling = NewLineHandling.Replace,
                OmitXmlDeclaration = true,
            });
            xNode.WriteTo(writer);
            writer.Flush();
            AddResultFile(context, file, ms.ToArray(), out outFile);
            context.WriteLine($"{file}: Attached");
        }
        else if (value is JsonNode jsonNode)
        {
            var file = changeExtension(composedFileName, ".xml");
            var ms = new MemoryStream();
            var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true, });
            jsonNode.WriteTo(writer);
            writer.Flush();
            AddResultFile(context, file, ms.ToArray(), out outFile);
            context.WriteLine($"{file}: Attached");
        }
        else if (value is Stream stream)
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            var file = changeExtension(composedFileName, ".bin");
            AddResultFile(context, file, ms.ToArray(), out outFile);
            context.WriteLine($"{file}: Attached");
        }
        else if (value is IEnumerable<string> lines)
        {
            var file = changeExtension(composedFileName, ".txt");
            AddResultFile(context, file, Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)), out outFile);
            context.WriteLine($"{file}: Attached");
        }
        else if (value is IEnumerable<KeyValuePair<string, string?>> items)
        {
            return AddResult(
                context,
                from item in items
                    //where !string.IsNullOrWhiteSpace(item.Value)
                select string.Join(",", $"\"{item.Key}\"", $"\"{item.Value}\""),
                 string.IsNullOrWhiteSpace(fileName) ? "Key Value Pair" : fileName,
                out outFile,
                caller,
                callerLine,
                callerFile
                );
        }
        else if (value is string content)
        {
            var file = changeExtension(composedFileName, ".txt");
            AddResultFile(context, file, Encoding.UTF8.GetBytes(content), out outFile);
            context.WriteLine($"{file}: Attached");
        }
        else if (value is IConfiguration config)
        {
            return AddResult(
                context,
                config.AsEnumerable(),
                 string.IsNullOrWhiteSpace(fileName) ? "Configuration" : fileName,
                out outFile,
                caller,
                callerLine,
                callerFile
                );

        }
        else if (value != null)
        {
            var file = changeExtension(composedFileName, ".json");

            var serializeOptions = DefaultJsonSerializer.DefaultOptions;
            serializeOptions.DefaultIgnoreCondition |=
                JsonIgnoreCondition.WhenWritingNull |
                JsonIgnoreCondition.WhenWritingDefault
                ;
            serializeOptions.WriteIndented = true;
            var serialize = new DefaultJsonSerializer(serializeOptions);

            var json = serialize.Serialize(value);
            AddResultFile(context, file, Encoding.UTF8.GetBytes(json), out outFile);
            context.WriteLine($"{file}: Attached");
        }
        else
        {
            outFile = null;
            context.WriteLine($"{composedFileName}: No output");
        }
        return context;
    }

    /// <summary>
    /// serialize an object to the test results for a given test run
    /// </summary>
    public static TestContext AddResultFile(this TestContext context, string fileName, byte[] content) =>
        context.AddResultFile(fileName, content, out var _);

    /// <summary>
    /// serialize an object to the test results for a given test run
    /// </summary>
    public static TestContext AddResultFile(this TestContext context, string fileName, byte[] content, out string outFile)
    {
        outFile = Path.Combine(context.TestRunResultsDirectory ?? ".", fileName);
        var dir = Path.GetDirectoryName(outFile);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllBytes(outFile, content);
        context.AddResultFile(outFile);
        return context;
    }

    /// <summary>
    /// deserialize test data from embedded resources
    /// </summary>
    /// <param name="context"></param>
    /// <param name="type"></param>
    /// <param name="target"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static object? GetTestData(this TestContext? context, Type type, string? target = null, IServiceProvider? serviceProvider = null) =>
        context.GetTestDataAsync(type, target, serviceProvider).GetAwaiter().GetResult();

    /// <summary>
    /// deserialize test data from embedded resources
    /// </summary>
    public static async Task<object?> GetTestDataAsync(this TestContext? context, Type type, string? target = null, IServiceProvider? serviceProvider = null)
    {
        if (type == null) return null;

        var typeQuery = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from assemblyType in assembly.GetTypes()
                        select assemblyType;
        var testType = typeQuery.FirstOrDefault(t => t.FullName == context?.FullyQualifiedTestClassName);

        var targetName = string.IsNullOrWhiteSpace(target) ? context?.TestName : target;

        if (string.IsNullOrWhiteSpace(target))
            target = null;

        var possibleResources = target != null ? new[] {
          $"{testType?.Name}_{context?.TestName}_{target}" ,
          $"{testType?.Name}_{context?.TestName}_{target}.json",
          $"{testType?.Name}_{target}" ,
          $"{testType?.Name}_{target}.json",

          $"{context?.TestName}_{target}",
          $"{context?.TestName}_{target}.json",
          $"{target}",
          $"{target}.json",
        } : new[]
        {
          $"{testType?.Name}_{context?.TestName}",
          $"{testType?.Name}_{context?.TestName}.json",
          $"{context?.TestName}",
          $"{context?.TestName}.json",
        }.Where(i => i != null);

        async Task<string?> getValueAsync()
        {
            foreach (var possible in possibleResources)
            {
                var result = await testType.GetResourceAsStringAsync(possible);
                if (!string.IsNullOrWhiteSpace(result))
                    return result;
            }
            return default;
        }
        Stream? getStream()
        {
            foreach (var possible in possibleResources)
            {
                var result = testType.GetResourceStream(possible);
                if (result != null)
                    return result;
            }
            return null;
        }
        if (type == typeof(Stream)) return getStream();

        var content = await getValueAsync();

        if (string.IsNullOrWhiteSpace(content)) return default;
        if (type == typeof(string)) return content;
        else if (type == typeof(JToken)) return JToken.Parse(content);
        else if (type == typeof(JObject)) return JObject.Parse(content);
        else if (type == typeof(JArray)) return JArray.Parse(content);

        var services = serviceProvider ??
            new ServiceCollection()
                .BuildServiceProvider()
                ;

        var result = JsonSerializer.Deserialize(content, type);
        return result;
    }

    /// <summary>
    /// deserialize test data from embedded resources
    /// </summary>
    public static async Task<T?> GetTestDataAsync<T>(this TestContext context, string? target = null, IServiceProvider? serviceProvider = null) where T : class =>
        (T?)await context.GetTestDataAsync(typeof(T), target, serviceProvider);

    /// <summary>
    /// get simplified name for executing test
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetQualifiedTestName(this TestContext context) =>
        $"{context.FullyQualifiedTestClassName}+{context?.TestName}";

    /// <summary>
    /// get path for test results folder for the executing test
    /// </summary>
    public static IEnumerable<string> GetTestRunResultFiles(this TestContext context) =>
        Directory.EnumerateDirectories(context.TestRunResultsDirectory ?? ".");

    /// <summary>
    /// get current type from test context
    /// </summary>
    /// <param name="testContext"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Type ResolveTestType(this TestContext testContext)
    {
        return resolveType() ?? throw new InvalidOperationException($"Unable to resolve type class for {testContext.FullyQualifiedTestClassName}");

        Type? resolveType()
        {
            var query = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from type in assembly.GetTypes()
                        where type.FullName == testContext.FullyQualifiedTestClassName
                        select type;
            return query.FirstOrDefault();
        }
    }
}
