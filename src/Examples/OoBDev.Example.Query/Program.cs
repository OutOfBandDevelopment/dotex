using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.Example.Query;
using OoBDev.PostgreSql.Query;
using OoBDev.Query.Metadata;

var builder = WebApplication.CreateBuilder(args);
builder.Services.TryAddPostgresMetadataQuery(builder.Configuration, typeof(NoteRow).Assembly);

var app = builder.Build();

app.MapGet("/describe", (KindRegistry registry) => Results.Json(Describer.Describe(registry)));
app.MapGet("/openapi.json", (KindRegistry registry) => Results.Content(OpenApiBuilder.Build(registry, "Notes query", "1.0").ToJsonString(), "application/json"));

// POST a QueryRequest, or GET ?text=... for the text form (for example: notes cats near geo:51.5,-0.1 max 25)
app.MapPost("/q", async (QueryRequest request, QueryEngine engine, CancellationToken ct) => await Run(() => engine.QueryAsync(request, ct)));
app.MapGet("/q", async (string text, QueryEngine engine, CancellationToken ct) =>
    await Run(() => engine.QueryAsync(QueryText.Parse(text, engine.Registry, "notes"), ct)));

app.Run();

static async Task<IResult> Run(Func<Task<QueryResult<IReadOnlyDictionary<string, object?>>>> query)
{
    try
    {
        return Results.Json(await query());
    }
    catch (QueryException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
    }
}
