using System;
using System.Threading.Tasks;
using Chatbot.Modules.Knowledge.Brokers.Storage;
using Chatbot.Modules.Knowledge.Models.Documents;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Integration.Brokers.Storage;

public class KnowledgeStorageBrokerTests(TestDatabaseFixture fixture)
    : IClassFixture<TestDatabaseFixture>
{
    private readonly StorageBroker _storageBroker = new(
        fixture.Configuration,
        new Chatbot.Shared.Infrastructure.Data.AuditInterceptor(SystemClock.Instance),
        new Chatbot.Shared.Infrastructure.Data.RlsInterceptor(NSubstitute.Substitute.For<ITenantProvider>())
    );

    [Fact]
    public void StorageBroker_ShouldUseKnowledgeSchema()
    {
        // Arrange & Act
        var docEntity = _storageBroker.Model.FindEntityType(typeof(KnowledgeDocument));
        var chunkEntity = _storageBroker.Model.FindEntityType(typeof(DocumentChunk));

        // Assert
        docEntity?.GetSchema().ShouldBe("knowledge");
        chunkEntity?.GetSchema().ShouldBe("knowledge");
    }

    [Fact]
    public void StorageBroker_ShouldUseSnakeCaseNamingConvention()
    {
        // Arrange
        var entityType = _storageBroker.Model.FindEntityType(typeof(KnowledgeDocument));
        entityType.ShouldNotBeNull();

        // Act
        var tableName = entityType!.GetTableName();
        var idProperty = entityType.FindProperty(nameof(KnowledgeDocument.Id));
        var sizeProperty = entityType.FindProperty(nameof(KnowledgeDocument.SizeInBytes));

        var storeObject = Microsoft.EntityFrameworkCore.Metadata.StoreObjectIdentifier.Table(
            tableName!,
            entityType.GetSchema()
        );
        var idColumnName = idProperty?.GetColumnName(storeObject);
        var sizeColumnName = sizeProperty?.GetColumnName(storeObject);

        // Assert
        tableName.ShouldBe("knowledge_documents");
        idColumnName.ShouldBe("id");
        sizeColumnName.ShouldBe("size_in_bytes");
    }

    [Fact]
    public async Task InsertKnowledgeDocumentAsync_ShouldInsertKnowledgeDocument()
    {
        // Arrange
        await ((DbContext)_storageBroker).Database.EnsureCreatedAsync();

        var now = SystemClock.Instance.GetCurrentInstant();
        var document = new KnowledgeDocument(
            Id: new KnowledgeDocumentId(Guid.NewGuid()),
            TenantId: new TenantId(Guid.NewGuid()),
            Title: "User Guide",
            FileName: "user_guide.pdf",
            BlobPath: "uploads/user_guide.pdf",
            SizeInBytes: 1024L * 50L,
            ContentType: "application/pdf",
            CreatedDate: now,
            UpdatedDate: now
        );

        // Act
        var insertedDoc = await _storageBroker.InsertKnowledgeDocumentAsync(document);

        // Assert
        insertedDoc.ShouldNotBeNull();
        insertedDoc.Id.ShouldBe(document.Id);

        var selectDoc = await _storageBroker.SelectKnowledgeDocumentByIdAsync(document.Id);
        selectDoc.ShouldNotBeNull();
        selectDoc!.Title.ShouldBe(document.Title);
    }

    [Fact]
    public async Task InsertDocumentChunkAsync_ShouldInsertDocumentChunk()
    {
        // Arrange
        await ((DbContext)_storageBroker).Database.EnsureCreatedAsync();

        var now = SystemClock.Instance.GetCurrentInstant();
        var docId = new KnowledgeDocumentId(Guid.NewGuid());
        var chunkId = new DocumentChunkId(Guid.NewGuid());
        var vectorId = Guid.NewGuid();

        var chunk = new DocumentChunk(
            Id: chunkId,
            DocumentId: docId,
            TenantId: new TenantId(Guid.NewGuid()),
            Index: 0,
            Content: "This is a section from the user guide...",
            VectorId: vectorId,
            CreatedDate: now,
            UpdatedDate: now
        );

        // Act
        var insertedChunk = await _storageBroker.InsertDocumentChunkAsync(chunk);

        // Assert
        insertedChunk.ShouldNotBeNull();
        insertedChunk.Id.ShouldBe(chunk.Id);

        var selectChunk = await _storageBroker.SelectDocumentChunkByIdAsync(chunk.Id);
        selectChunk.ShouldNotBeNull();
        selectChunk!.Content.ShouldBe(chunk.Content);
        selectChunk.VectorId.ShouldBe(vectorId);
    }
}
