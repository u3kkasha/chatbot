using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chatbot.Shared.Brokers.Ai;
using Chatbot.Shared.Infrastructure.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Unit.Brokers.Ai;

public class AiBrokerTests
{
    private readonly IChatClient _chatClient;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly IOpenRouterClient _openRouterClient;
    private readonly IOptions<AiOptions> _aiOptions;
    private readonly AiBroker _sut;

    public AiBrokerTests()
    {
        _chatClient = Substitute.For<IChatClient>();
        _embeddingGenerator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        _openRouterClient = Substitute.For<IOpenRouterClient>();

        var options = new AiOptions { RerankModelId = "cohere/rerank-v3.5" };
        _aiOptions = Options.Create(options);

        _sut = new AiBroker(_chatClient, _embeddingGenerator, _openRouterClient, _aiOptions);
    }

    [Fact]
    public async Task ShouldGetChatCompletionAsync()
    {
        // given
        const string prompt = "Hello";
        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, "Hi"));
        _chatClient.GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions>())
            .Returns(Task.FromResult(response));

        // when
        var result = await _sut.GetChatCompletionAsync(prompt);

        // then
        result.ShouldBe("Hi");
        await _chatClient.Received(1).GetResponseAsync(
            Arg.Is<IEnumerable<ChatMessage>>(m => m.Last().Text == prompt),
            Arg.Any<ChatOptions>());
    }

    [Fact]
    public async Task ShouldGenerateEmbeddingAsync()
    {
        // given
        const string text = "test text";
        var vector = new float[] { 1.0f, 2.0f, 3.0f };
        var embedding = new Embedding<float>(vector);
        var response = new GeneratedEmbeddings<Embedding<float>>([embedding]);

        _embeddingGenerator.GenerateAsync(Arg.Is<IEnumerable<string>>(s => s.Contains(text)), Arg.Any<EmbeddingGenerationOptions>())
            .Returns(Task.FromResult(response));

        // when
        var result = await _sut.GenerateEmbeddingAsync(text);

        // then
        result.ShouldBe(vector);
        await _embeddingGenerator.Received(1).GenerateAsync(
            Arg.Is<IEnumerable<string>>(s => s.Contains(text)),
            Arg.Any<EmbeddingGenerationOptions>());
    }
}
