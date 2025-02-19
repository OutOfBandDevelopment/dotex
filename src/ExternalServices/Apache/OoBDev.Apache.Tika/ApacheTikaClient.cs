using Microsoft.Extensions.Logging;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika;

public class ApacheTikaClient : IApacheTikaClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public ApacheTikaClient(
        HttpClient httpClient,
        ILogger<ApacheTikaClient> logger
            )
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async ValueTask<string> DetectStreamAsync(Stream source)
    {
        _logger.LogInformation("Detecting Content Type");
        var response = await _httpClient.PutAsync("/detect/stream", new StreamContent(source));
        var result = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Detected Content Type: {contentType}", result);
        return result;
    }

    public async Task ConvertAsync(Stream source, string sourceContentType, Stream destination, string destinationContentType)
    {
        _logger.LogInformation("Convert: {source} -> {destination}", sourceContentType, destinationContentType);
        var request = new HttpRequestMessage(HttpMethod.Put, "/tika")
        {
            Content = new StreamContent(source),
        };
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(destinationContentType));

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        using var stream = response.Content.ReadAsStream();
        stream.CopyTo(destination);
        destination.Position = 0;
    }

    public async Task<string> GetHelloAsync()
    {
        var response = await _httpClient.GetAsync("/tika");
        var result = await response.Content.ReadAsStringAsync();
        return result;
    }
}
