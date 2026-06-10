using System;
using System.Linq;
using Chatbot.Modules.Knowledge.Services;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Unit.Modules.Knowledge.Services;

public class SparseVectorServiceTests
{
    private readonly ISparseVectorService sparseVectorService;

    public SparseVectorServiceTests()
    {
        this.sparseVectorService = new SparseVectorService();
    }

    [Fact]
    public void ShouldGenerateSparseVectorForValidText()
    {
        // given
        string text = "The quick brown fox jumps over the lazy dog";

        // when
        var (indices, values) = this.sparseVectorService.GenerateAsync(text);

        // then
        indices.ShouldNotBeEmpty();
        values.ShouldNotBeEmpty();
        indices.Length.ShouldBe(tf().Keys.Count);
        values.Length.ShouldBe(tf().Keys.Count);

        // Verify some stable hashes (djb2)
        uint GetHash(string t)
        {
            uint hash = 5381;
            foreach (char c in t) hash = ((hash << 5) + hash) + (uint)c;
            return hash;
        }

        indices.ShouldContain(GetHash("quick"));
        indices.ShouldContain(GetHash("brown"));
        
        // "the" has length 3, so it should be included
        indices.ShouldContain(GetHash("the"));

        Dictionary<string, float> tf() => text.ToLowerInvariant().Split(' ')
            .Where(t => t.Length > 2)
            .GroupBy(t => t)
            .ToDictionary(g => g.Key, g => (float)g.Count());
    }

    [Fact]
    public void ShouldReturnEmptyVectorsForEmptyText()
    {
        // given
        string text = "";

        // when
        var (indices, values) = this.sparseVectorService.GenerateAsync(text);

        // then
        indices.ShouldBeEmpty();
        values.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldReturnEmptyVectorsForShortWordsOnly()
    {
        // given
        string text = "a to in of";

        // when
        var (indices, values) = this.sparseVectorService.GenerateAsync(text);

        // then
        indices.ShouldBeEmpty();
        values.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldBeCaseInsensitive()
    {
        // given
        string text1 = "Chatbot";
        string text2 = "chatbot";

        // when
        var result1 = this.sparseVectorService.GenerateAsync(text1);
        var result2 = this.sparseVectorService.GenerateAsync(text2);

        // then
        result1.indices.ShouldBe(result2.indices);
        result1.values.ShouldBe(result2.values);
    }
}
