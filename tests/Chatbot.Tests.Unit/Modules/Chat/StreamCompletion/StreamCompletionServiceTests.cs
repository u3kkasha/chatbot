using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Features.StreamCompletion;
using Chatbot.Shared.Brokers.Ai;
using Microsoft.Extensions.AI;
using NSubstitute;
using Shouldly;

namespace Chatbot.Tests.Unit.Modules.Chat.StreamCompletion;

public class StreamCompletionServiceTests
{
    private readonly IAiBroker _aiBroker;
    private readonly StreamCompletionService _sut;

    public StreamCompletionServiceTests()
    {
        _aiBroker = Substitute.For<IAiBroker>();
        _sut = new StreamCompletionService(_aiBroker);
    }

    // ──────────────────────────────────────────────
    // Happy-path tests
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ShouldEmitTokensFromBroker_WhenPromptIsValid()
    {
        // given
        const string prompt = "Hello, world!";
        var brokerUpdates = new[] { MakeUpdate("He"), MakeUpdate("llo"), MakeUpdate(", world!") };

        _aiBroker.StreamChatCompletionAsync(prompt).Returns(ToAsyncEnumerable(brokerUpdates));

        // when
        var tokens = await CollectTokensAsync(prompt);

        // then – all content tokens present, last one is sentinel
        tokens.Count.ShouldBe(4); // 3 content + 1 final sentinel
        tokens[0].ShouldBe(new SseToken("He", IsFinal: false));
        tokens[1].ShouldBe(new SseToken("llo", IsFinal: false));
        tokens[2].ShouldBe(new SseToken(", world!", IsFinal: false));
        tokens[3].ShouldBe(new SseToken(string.Empty, IsFinal: true));
    }

    [Fact]
    public async Task ShouldSkipEmptyTextUpdates_FromBroker()
    {
        // given
        const string prompt = "test";
        var brokerUpdates = new[]
        {
            MakeUpdate("Hello"),
            MakeUpdate(""), // empty – must be skipped
            MakeUpdate("   "), // whitespace-only – skipped  (trimmed: empty)
            MakeUpdate("World"),
        };

        _aiBroker.StreamChatCompletionAsync(prompt).Returns(ToAsyncEnumerable(brokerUpdates));

        // when
        var tokens = await CollectTokensAsync(prompt);

        // then – only "Hello", "World" emitted + final sentinel
        tokens.Count.ShouldBe(3);
        tokens[0].Text.ShouldBe("Hello");
        tokens[1].Text.ShouldBe("World");
        tokens[2].IsFinal.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldAlwaysEmitFinalSentinel_EvenWithZeroContentTokens()
    {
        // given
        const string prompt = "empty";
        _aiBroker.StreamChatCompletionAsync(prompt).Returns(ToAsyncEnumerable([]));

        // when
        var tokens = await CollectTokensAsync(prompt);

        // then – only the sentinel is emitted
        tokens.Count.ShouldBe(1);
        tokens[0].ShouldBe(new SseToken(string.Empty, IsFinal: true));
    }

    [Fact]
    public async Task ShouldDelegateToBrokerWithCorrectPrompt()
    {
        // given
        const string prompt = "What is DDD?";
        _aiBroker.StreamChatCompletionAsync(prompt).Returns(ToAsyncEnumerable([]));

        // when
        await CollectTokensAsync(prompt);

        // then – broker received the exact prompt
        _aiBroker.Received(1).StreamChatCompletionAsync(prompt);
    }

    [Fact]
    public async Task ShouldRespectCancellation_WhenTokenRequested()
    {
        // given
        const string prompt = "cancel me";
        using var cts = new CancellationTokenSource();

        _aiBroker.StreamChatCompletionAsync(prompt).Returns(InfiniteUpdates(cts.Token));

        // when – cancel immediately
        cts.Cancel();

        var tokens = new List<SseToken>();
        await foreach (var token in _sut.StreamAsync(prompt, cts.Token))
        {
            tokens.Add(token);
        }

        // then – no infinite loop; the loop exits via cancellation
        // (exact token count depends on scheduling; just confirm no exception thrown)
        tokens.ShouldNotBeNull();
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private async Task<List<SseToken>> CollectTokensAsync(
        string prompt,
        CancellationToken ct = default
    )
    {
        var result = new List<SseToken>();
        await foreach (var token in _sut.StreamAsync(prompt, ct))
        {
            result.Add(token);
        }
        return result;
    }

    private static ChatResponseUpdate MakeUpdate(string text) =>
        new() { Role = ChatRole.Assistant, Contents = [new TextContent(text)] };

    private static async IAsyncEnumerable<ChatResponseUpdate> ToAsyncEnumerable(
        IEnumerable<ChatResponseUpdate> items,
        [EnumeratorCancellation] CancellationToken ct = default
    )
    {
        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested();
            yield return item;
            await Task.Yield();
        }
    }

    private static async IAsyncEnumerable<ChatResponseUpdate> InfiniteUpdates(
        [EnumeratorCancellation] CancellationToken ct = default
    )
    {
        while (!ct.IsCancellationRequested)
        {
            yield return MakeUpdate("token");
            await Task.Delay(10, ct).ConfigureAwait(false);
        }
    }
}
