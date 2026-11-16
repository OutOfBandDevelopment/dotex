using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace OoBDev.Query.Metadata.Tests;

internal static class Support
{
    public const string Key = "0123456789abcdef0123456789abcdef-test-key";

    public static readonly KindRegistry Registry = new(typeof(PhotoRow).Assembly);

    public static IOptions<QueryMetadataOptions> Options(string key = Key) => Microsoft.Extensions.Options.Options.Create(new QueryMetadataOptions { CursorKey = key });

    public static CursorCodec Codec(string key = Key) => new(Options(key));

    public static Dictionary<string, JsonElement> Filters(string json) => JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;

    public static CompiledQuery Compile(QueryRequest request, string? key = null) =>
        QueryCompiler.Compile(Registry.Get(request.From), request, new TestDialect(), 500, Codec(key ?? Key));
}
