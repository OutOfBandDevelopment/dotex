using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OoBDev.AspNetCore.Authorization;
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
    public const string DefaultDetailRight = "health.details";

    /// <summary>Resolves what the caller may see: status only when anonymous, the check list when authenticated, everything with the detail right.</summary>
    internal static HealthCheckDetailLevel GetLevel(HttpContext context, string detailRight)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return HealthCheckDetailLevel.Status;

        var checker = context.RequestServices.GetService<IApplicationRightChecker>();
        return checker?.Check(context.User, [detailRight]) == RightCheckResult.Allowed
            ? HealthCheckDetailLevel.Details
            : HealthCheckDetailLevel.Checks;
    }

    public static HealthCheckOptions Create(string detailRight = DefaultDetailRight) =>
        new()
        {
            Predicate = _ => true,

            ResponseWriter = async (context, report) =>
            {
                // https://blog.kritner.com/2020/12/04/prettifying-healthchecks/
                context.Response.ContentType = "application/json; charset=utf-8";
                var level = GetLevel(context, detailRight);

                var options = new JsonWriterOptions
                {
                    Indented = true
                };

                var json = Write(report, options, level);
                await context.Response.WriteAsync(json);
            },

            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        };

    internal static string Write(HealthReport report, JsonWriterOptions options, HealthCheckDetailLevel level)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, options))
        {
            writer.WriteStartObject();
            writer.WriteString("status", report.Status.ToString());
            if (level == HealthCheckDetailLevel.Status)
            {
                writer.WriteEndObject();
                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray());
            }

            writer.WriteStartObject("results");
            foreach (var entry in report.Entries)
            {
                writer.WriteStartObject(entry.Key);
                writer.WriteString("status", entry.Value.Status.ToString());

                if (level == HealthCheckDetailLevel.Details && entry.Value.Description is not null)
                    writer.WriteString("description", entry.Value.Description);

                if (level == HealthCheckDetailLevel.Details && entry.Value.Data is not null && entry.Value.Data.Count > 0)
                {
                    writer.WriteStartObject("data");
                    foreach (var item in entry.Value.Data)
                    {
                        writer.WritePropertyName(item.Key);
                        JsonSerializer.Serialize(writer, item.Value, item.Value?.GetType() ?? typeof(object));
                    }
                    writer.WriteEndObject();
                }

                if (level == HealthCheckDetailLevel.Details && entry.Value.Exception?.Message is not null)
                    writer.WriteString("exception", entry.Value.Exception?.Message);

                writer.WriteEndObject();
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
