Overall, the code looks good, but there are several areas where improvements can be made to enhance maintainability, scalability, and readability. Here are my suggestions:

1. **Factor out unnecessary logic**: The `HealthCheckOptionsFactory` class is responsible for creating and configuring health check options. However, the logic for writing the JSON response is tightly coupled with this class. It would be better to extract this logic into a separate class, e.g., `HealthCheckJsonWriter`. This would make the code more modular and easier to test.
2. **Remove repetitive code**: The code has several repetitive sections for writing the JSON response. You can reduce this repetition by extracting common logic into a separate method, e.g., `WriteJsonEntry`.
3. **Improve naming conventions**: Some variable names, such as `_` and `report`, are not descriptive. Consider renaming them to something more meaningful, e.g., `healthCheckStatus` and `healthCheckReport`.
4. **Use generic types**: The `JsonSerializer.Serialize` method is called with a generic type parameter `Object`. You can change this to a generic type parameter `T` to make the code more flexible and type-safe.
5. **Consider using a caching mechanism**: The `HealthCheckOptionsFactory` class creates new instances of `HealthCheckOptions` each time it's called. If you're experiencing performance issues, you can consider caching the instances to improve performance.
6. **Split large methods**: The `Create` method is quite long and complex. You can split it into smaller methods to make it easier to read and maintain.
7. **Consider using a config file or settings service**: The `HealthCheckOptionsFactory` class hardcodes some settings, such as response codes and JSON options. You can consider moving these settings to a config file or settings service to make the code more configurable.

Here's an updated version of the code incorporating these suggestions:
```csharp
public class HealthCheckJsonWriter
{
    public static async Task WriteAsync(HttpResponse response, HealthCheckReport report)
    {
        // ...

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, _jsonOptions))
        {
            writer.WriteStartObject();
            writer.WriteString("status", report.Status.ToString());
            writer.WriteStartObject("results");
            foreach (var entry in report.Entries)
            {
                WriteJsonEntry(writer, entry);
            }
            writer.WriteEndObject();
        }

        var json = Encoding.UTF8.GetString(stream.ToArray());

        await response.WriteAsync(json);
    }

    private static void WriteJsonEntry(Utf8JsonWriter writer, (string Key, HealthCheckEntry Value) entry)
    {
        writer.WriteStartObject(entry.Key);
        writer.WriteString("status", entry.Value.Status.ToString());

        if (entry.Value.Description is not null)
            writer.WriteString("description", entry.Value.Description);

        if (entry.Value.Data is not null && entry.Value.Data.Count > 0)
        {
            writer.WriteStartObject("data");
            foreach (var item in entry.Value.Data)
            {
                writer.WritePropertyName(item.Key);
                JsonSerializer.Serialize(writer, item.Value, item.Value.GetType());
            }
            writer.WriteEndObject();
        }

        if (entry.Value.Exception?.Message is not null)
            writer.WriteString("exception", entry.Value.Exception?.Message);

        writer.WriteEndObject();
    }
}
```

```csharp
public class HealthCheckOptionsFactory
{
    private readonly HealthCheckJsonWriter _jsonWriter;

    public HealthCheckOptionsFactory(HealthCheckJsonWriter jsonWriter)
    {
        _jsonWriter = jsonWriter;
    }

    public static HealthCheckOptions Create()
    {
        var options = new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = async (context, report) => _jsonWriter.WriteAsync(context.Response, report),
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        };

        return options;
    }
}
```
Note that this is just a suggested update, and you may need to adjust the code to fit your specific requirements.