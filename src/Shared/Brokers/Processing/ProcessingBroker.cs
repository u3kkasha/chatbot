using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Chatbot.Shared.Brokers.Processing.Models;

namespace Chatbot.Shared.Brokers.Processing;

public class ProcessingBroker(IDoclingClient doclingClient) : IProcessingBroker
{
    public async ValueTask<IReadOnlyList<DocumentChunk>> ChunkDocumentAsync(Stream document, string fileName)
    {
        var response = await doclingClient.ProcessAsync(document, fileName);

        if (response.Document.JsonContent?.Texts != null)
        {
            return response.Document.JsonContent.Texts;
        }

        if (!string.IsNullOrEmpty(response.Document.MdContent))
        {
            // Fallback: Split Markdown by double newline (paragraphs/sections)
            return response.Document.MdContent
                .Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(text => new DocumentChunk(text.Trim(), "fallback-paragraph"))
                .ToList();
        }

        return new List<DocumentChunk>();
    }
}
