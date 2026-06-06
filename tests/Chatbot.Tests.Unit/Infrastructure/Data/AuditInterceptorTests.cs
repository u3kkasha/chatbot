using Chatbot.Shared.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NSubstitute;

namespace Chatbot.Tests.Unit.Infrastructure.Data;

public class AuditInterceptorTests
{
    private class TestEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Instant CreatedDate { get; set; }
        public Instant UpdatedDate { get; set; }
    }

    private class TestDbContext : DbContext
    {
        private readonly AuditInterceptor _interceptor;

        public TestDbContext(DbContextOptions<TestDbContext> options, AuditInterceptor interceptor)
            : base(options)
        {
            _interceptor = interceptor;
        }

        public DbSet<TestEntity> Entities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(_interceptor);
        }
    }

    [Fact]
    public async Task AuditInterceptor_ShouldSetDatesOnAddedEntity()
    {
        // Arrange
        var mockClock = Substitute.For<IClock>();
        var instant = Instant.FromUtc(2026, 6, 6, 12, 0, 0);
        mockClock.GetCurrentInstant().Returns(instant);

        var interceptor = new AuditInterceptor(mockClock);
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options, interceptor);

        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        context.Entities.Add(entity);
        await context.SaveChangesAsync();

        // Assert
        entity.CreatedDate.Should().Be(instant);
        entity.UpdatedDate.Should().Be(instant);
    }

    [Fact]
    public async Task AuditInterceptor_ShouldUpdateOnlyModifiedDateOnModifiedEntity()
    {
        // Arrange
        var mockClock = Substitute.For<IClock>();
        var initialInstant = Instant.FromUtc(2026, 6, 6, 12, 0, 0);
        var modifiedInstant = Instant.FromUtc(2026, 6, 6, 13, 0, 0);

        mockClock.GetCurrentInstant().Returns(initialInstant);

        var interceptor = new AuditInterceptor(mockClock);
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using (var context = new TestDbContext(options, interceptor))
        {
            var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Initial" };
            context.Entities.Add(entity);
            await context.SaveChangesAsync();
        }

        // Act & Assert
        mockClock.GetCurrentInstant().Returns(modifiedInstant);

        using (var context = new TestDbContext(options, interceptor))
        {
            var entity = await context.Entities.FirstAsync();
            entity.Name = "Updated";
            context.Entities.Update(entity);
            await context.SaveChangesAsync();

            entity.CreatedDate.Should().Be(initialInstant); // Stays the same
            entity.UpdatedDate.Should().Be(modifiedInstant); // Updated to new instant
        }
    }
}
