using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Chatbot.Shared.Brokers.Ai.Models;

namespace Chatbot.Shared.Brokers.Ai;

public interface IOpenRouterClient
{
    Task<RerankResponse> RerankAsync(RerankRequest request);
}

public class OpenRouterClient(HttpClient httpClient) : IOpenRouterClient
{
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Not compiled with PublishAot=true")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Not compiled with PublishAot=true")]
    public async Task<RerankResponse> RerankAsync(RerankRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("rerank", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RerankResponse>())!;
    }
}
