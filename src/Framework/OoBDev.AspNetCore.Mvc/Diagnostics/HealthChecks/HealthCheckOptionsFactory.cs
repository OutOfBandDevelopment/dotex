using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OoBDev.AspNetCore.Mvc.Diagnostics.HealthChecks;

/// <summary>
/// Factory class for creating health check options.
/// </summary>
public class HealthCheckOptionsFactory
{
    /// <summary>
    /// Creates and configures a new instance of <see cref="HealthCheckOptions"/>.
    /// </summary>
    /// <returns>The configured <see cref="HealthCheckOptions"/> instance.</returns>
    public static HealthCheckOptions Create() =>
        new()
        {
            Predicate = _ => true,

            ResponseWriter = async (context, report) =>
            {
                // https://blog.kritner.com/2020/12/04/prettifying-healthchecks/
                context.Response.ContentType = "application/json; charset=utf-8";

                //TODO: make it so unauthenticated only gets the "status"
                //TODO: authenticates should get result list without descriptions
                //TODO: special claim gets descriptions, data and errors

                var options = new JsonWriterOptions
                {
                    Indented = true
                };

                using var stream = new MemoryStream();
                using (var writer = new Utf8JsonWriter(stream, options))
                {
                    writer.WriteStartObject();
                    writer.WriteString("status", report.Status.ToString());
                    writer.WriteStartObject("results");
                    foreach (var entry in report.Entries)
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
                                JsonSerializer.Serialize(writer, item.Value, item.Value?.GetType() ?? typeof(object));
                            }
                            writer.WriteEndObject();
                        }

                        if (entry.Value.Exception?.Message is not null)
                            writer.WriteString("exception", entry.Value.Exception?.Message);

                        writer.WriteEndObject();
                    }
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }

                var json = Encoding.UTF8.GetString(stream.ToArray());

                await context.Response.WriteAsync(json);
            },

            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        };
}
