using System;
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

        var streamContent = new StreamContent(document);
        content.Add(streamContent, "files", fileName);

        // Add options to request JSON and MD formats
        var optionsContent = new StringContent("{\"to_formats\": [\"json\", \"md\"]}", System.Text.Encoding.UTF8, "application/json");
        content.Add(optionsContent, "options");

        var response = await httpClient.PostAsync("v1/convert/file", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Docling conversion failed with status {response.StatusCode}: {error}");
        }

        var json = await response.Content.ReadAsStringAsync();
        try
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var result = System.Text.Json.JsonSerializer.Deserialize<ProcessingResponse>(json, options);

            if (result?.Document == null || (result.Document.JsonContent == null && string.IsNullOrEmpty(result.Document.MdContent)))
            {
                throw new InvalidOperationException($"Docling response mapping failed. JSON: {json}");
            }

            return result;
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize Docling response: {json}", ex);
        }
    }
}
