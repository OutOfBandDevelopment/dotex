using OoBDev.Common;
using OoBDev.Common.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Extensions.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLoggingMiddleware(
        RequestDelegate next
        ) => _next = next;

    public async Task Invoke(
       HttpContext context,
       ILogger<AuditLoggingMiddleware> logger,
       IAccessor<CorrelationInfo> correlationAccessor,
       IAuditLoggingRecorder recorder
       )
    {
        var auditable = context.GetEndpoint()?.Metadata.OfType<AuditRequestAttribute>().FirstOrDefault();

        if (auditable is null) // not auditable endpoint so move along
        {
            await _next.Invoke(context);
            return;
        }

        context.Request.EnableBuffering();

        var requestBody = auditable.ExcludeRequest ? null : await context.GetRequestBodyAsync();

        var requestId = correlationAccessor.Value?.RequestId;
        var correlationId = correlationAccessor.Value?.CorrelationId;
        var appName = context.Request.RouteValues["controller"]?.ToString() ?? GetType().Assembly.FullName ?? "Unknown";

        var identifier = await context.GetIdentifierAsync();

        //swap out body to buffer locally so it may be captured
        var originalResponse = context.Response.Body;
        var bufferedResponse = new MemoryStream();
        context.Response.Body = bufferedResponse;

        var entry = new AuditLogEntry
        {
            ApplicationName = appName,
            ApplicationSessionID = correlationId ?? Guid.NewGuid().ToString(),
            Description = context.Request.Path,

            IPAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            LogTypeName = string.IsNullOrWhiteSpace(auditable.LogTypeName) ? AuditRequestAttribute.DefaultLogTypeName : auditable.LogTypeName,

            Identifier = identifier.identifier,
            IdentifierValue = identifier.value,

            AuditInfo = new AuditLogInfo
            {
                Parameters = MakeObject(requestBody),
                ServiceCall = context.Request.Path,

                TimeStamp = DateTimeOffset.Now,

                RequestId = requestId,
                CorrelationId = correlationId,
            }
        };

        try
        {
            context.Request.Body.Position = 0;
            await _next.Invoke(context); //run the rest of the pipeline

            var responseBody = auditable.ExcludeResponse ? null : await context.GetResponseBodyAsync();

            entry = entry with
            {
                AuditInfo = entry.AuditInfo with
                {
                    StatusCode = context.Response.StatusCode,
                    Response = MakeObject(responseBody),
                }
            };
        }
        catch (Exception ex)
        {
            entry = entry with
            {
                AuditInfo = entry.AuditInfo with
                {
                    StatusCode = context.Response.StatusCode,
                    Response = new ErrorResult { Error = ex.Message, Data = ex.Data, StackTrace = ex.StackTrace },
                }
            };
            throw;
        }
        finally
        {
            await foreach (var logId in recorder.RecordAsync(entry))
            {
                logger.LogInformation(
                    $"Audit: {{{nameof(entry.Description)}}}({{{nameof(entry.ApplicationSessionID)}}}) => {{{nameof(logId)}}}",
                    entry.Description,
                    entry.ApplicationSessionID,
                    logId);
            }
            var currentPosition = bufferedResponse.Position;
            bufferedResponse.Position = 0;
            await bufferedResponse.CopyToAsync(originalResponse);
            bufferedResponse.Position = currentPosition;
            context.Response.Body = originalResponse;
        }
    }

    private record ErrorResult
    {
        public required string Error { get; init; }
        public required IDictionary Data { get; init; }
        public string? StackTrace { get; init; }
    }

    private object? MakeObject(object? input)
    {
        try
        {
            if (input is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
                return JsonObject.Parse(stringValue) ?? throw new NotSupportedException("there is no try parse... sorry");
        }
        catch
        {
            //note: if there is a parser error just eat it and return the string
        }
        return input;
    }
}
