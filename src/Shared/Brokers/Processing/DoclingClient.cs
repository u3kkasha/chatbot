using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Chatbot.Shared.Brokers.Processing.Models;

namespace Chatbot.Shared.Brokers.Processing;

public interface IDoclingClient
{
    Task<ProcessingResponse> ProcessAsync(Stream document, string fileName);
}

public class DoclingClient(HttpClient httpClient) : IDoclingClient
{
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Not compiled with PublishAot=true")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Not compiled with PublishAot=true")]
    public async Task<ProcessingResponse> ProcessAsync(Stream document, string fileName)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(document);
        content.Add(streamContent, "file", fileName);

        var response = await httpClient.PostAsync("process", content);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProcessingResponse>())!;
    }
}
