using Chatbot.Shared.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Chatbot.Tests.Unit.Infrastructure.Data;

public class AuditInterceptorTests
{
    private class TestEntity
    {
        public Guid Id { get; set; }
        public Instant CreatedDate { get; set; }
        public Instant UpdatedDate { get; set; }
    }

    private class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestEntity> Entities { get; set; }
    }

    [Fact]
    public async Task AuditInterceptor_ShouldSetDatesOnAddedEntity()
    {
        // This is a complex test, we'll need to mock the DbContext and Interceptor
        // Or better, use an In-Memory DB with the interceptor configured.
    }
}
