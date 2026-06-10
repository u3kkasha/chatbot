using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Chatbot.Modules.Knowledge.Models;
using Chatbot.Modules.Knowledge.Models.Errors;
using Chatbot.Modules.Knowledge.Services;
using Chatbot.Shared.Brokers.Ai;
using Chatbot.Shared.Brokers.Logging;
using Chatbot.Shared.Brokers.Vectors;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Unit.Modules.Knowledge.Services;

public class KnowledgeFoundationServiceTests
{
    private readonly IAiBroker aiBrokerMock;
    private readonly ISparseVectorService sparseVectorServiceMock;
    private readonly IQdrantBroker qdrantBrokerMock;
    private readonly ILoggingBroker loggingBrokerMock;
    private readonly IKnowledgeFoundationService knowledgeFoundationService;

    public KnowledgeFoundationServiceTests()
    {
        this.aiBrokerMock = Substitute.For<IAiBroker>();
        this.sparseVectorServiceMock = Substitute.For<ISparseVectorService>();
        this.qdrantBrokerMock = Substitute.For<IQdrantBroker>();
        this.loggingBrokerMock = Substitute.For<ILoggingBroker>();

        this.knowledgeFoundationService = new KnowledgeFoundationService(
            this.aiBrokerMock,
            this.sparseVectorServiceMock,
            this.qdrantBrokerMock,
            this.loggingBrokerMock);
    }

    [Fact]
    public async Task ShouldRetrieveKnowledgeChunksAsync()
    {
        // given
        string query = "What is RAG?";
        float[] denseVector = [0.1f, 0.2f];
        (uint[] indices, float[] values) sparseVector = ([1u], [1.0f]);
        
        var candidates = new List<QdrantSearchPoint>
        {
            new(1, "RAG stands for Retrieval-Augmented Generation.", new Dictionary<string, object?> { ["documentId"] = "doc1" }, 0.9f)
        };

        var rerankResults = new List<RerankResult>
        {
            new(0, "RAG stands for Retrieval-Augmented Generation.", 0.95f)
        };

        this.aiBrokerMock.GenerateEmbeddingAsync(query).Returns(denseVector);
        this.sparseVectorServiceMock.GenerateAsync(query).Returns(sparseVector);
        this.qdrantBrokerMock.HybridSearchAsync("knowledge", denseVector, sparseVector, 50).Returns(candidates);
        this.aiBrokerMock.RerankAsync(query, Arg.Any<IEnumerable<string>>(), 5).Returns(rerankResults);

        // when
        var result = await this.knowledgeFoundationService.RetrieveAsync(query);

        // then
        var actualChunks = result.AsT0;
        actualChunks.Count.ShouldBe(1);
        actualChunks[0].Content.ShouldBe("RAG stands for Retrieval-Augmented Generation.");
        actualChunks[0].DocumentId.ShouldBe("doc1");
        actualChunks[0].Score.ShouldBe(0.95f);

        await this.aiBrokerMock.Received(1).GenerateEmbeddingAsync(query);
        this.sparseVectorServiceMock.Received(1).GenerateAsync(query);
        await this.qdrantBrokerMock.Received(1).HybridSearchAsync("knowledge", denseVector, sparseVector, 50);
        await this.aiBrokerMock.Received(1).RerankAsync(query, Arg.Any<IEnumerable<string>>(), 5);
    }

    [Fact]
    public async Task ShouldReturnDependencyErrorWhenBrokerThrowsDependencyException()
    {
        // given
        string query = "What is RAG?";
        var exception = new HttpRequestException("Connection timeout");
        this.aiBrokerMock.GenerateEmbeddingAsync(query).Throws(exception);

        // when
        var result = await this.knowledgeFoundationService.RetrieveAsync(query);

        // then
        result.IsT2.ShouldBeTrue(); // DependencyError
        result.AsT2.Message.ShouldBe("A dependency error occurred during knowledge retrieval.");
        result.AsT2.InnerException.ShouldBe(exception);
        this.loggingBrokerMock.Received(1).LogError(exception);
    }

    [Fact]
    public async Task ShouldReturnServiceErrorWhenBrokerThrowsGeneralException()
    {
        // given
        string query = "What is RAG?";
        var exception = new Exception("General failure");
        this.aiBrokerMock.GenerateEmbeddingAsync(query).Throws(exception);

        // when
        var result = await this.knowledgeFoundationService.RetrieveAsync(query);

        // then
        result.IsT1.ShouldBeTrue(); // ServiceError
        result.AsT1.Message.ShouldBe("A service error occurred during knowledge retrieval.");
        result.AsT1.InnerException.ShouldBe(exception);
        this.loggingBrokerMock.Received(1).LogError(exception);
    }
}
