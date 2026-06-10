using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Chatbot.Modules.Knowledge.Services;

public interface ISparseVectorService
{
    (uint[] indices, float[] values) GenerateAsync(string text);
}

public partial class SparseVectorService : ISparseVectorService
{
    [GeneratedRegex(@"\W+")]
    private static partial Regex TokenizerRegex();

    public (uint[] indices, float[] values) GenerateAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (Array.Empty<uint>(), Array.Empty<float>());

        var tokens = TokenizerRegex().Split(text.ToLowerInvariant())
            .Where(t => t.Length > 2)
            .ToList();

        if (tokens.Count == 0)
            return (Array.Empty<uint>(), Array.Empty<float>());

        var tf = tokens.GroupBy(t => t)
            .ToDictionary(g => g.Key, g => (float)g.Count());

        var sortedIndices = tf.Keys
            .Select(t => (token: t, id: GetTokenId(t)))
            .OrderBy(x => x.id)
            .ToList();

        var indices = sortedIndices.Select(x => x.id).ToArray();
        var values = sortedIndices.Select(x => tf[x.token]).ToArray();

        return (indices, values);
    }

    private static uint GetTokenId(string token)
    {
        // Stable hash for token (djb2)
        uint hash = 5381;
        foreach (char c in token)
        {
            hash = ((hash << 5) + hash) + (uint)c;
        }
        return hash;
    }
}
