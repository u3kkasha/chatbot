using System;
using System.Data.Common;
using Chatbot.Shared.Infrastructure.Data;
using Chatbot.Shared.Models;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Unit.Infrastructure.Data;

public class RlsInterceptorTests
{
    private readonly ITenantProvider _tenantProviderMock;
    private readonly RlsInterceptor _interceptor;

    public RlsInterceptorTests()
    {
        _tenantProviderMock = Substitute.For<ITenantProvider>();
        _interceptor = new RlsInterceptor(_tenantProviderMock);
    }

    [Fact]
    public void SetTenantId_ShouldInjectTenantId_WhenTenantIdIsPresent()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantProviderMock.GetTenantId().Returns(tenantId);

        var commandMock = Substitute.For<DbCommand>();
        commandMock.CommandText = "SELECT * FROM Users";

        // Act
        _interceptor.SetTenantId(commandMock);

        // Assert
        commandMock.CommandText.ShouldStartWith($"SET LOCAL app.current_tenant_id = '{tenantId}';");
    }

    [Fact]
    public void SetTenantId_ShouldNotInjectTenantId_WhenTenantIdIsMissing()
    {
        // Arrange
        _tenantProviderMock.GetTenantId().Returns((Guid?)null);

        var commandMock = Substitute.For<DbCommand>();
        commandMock.CommandText = "SELECT * FROM Users";

        // Act
        _interceptor.SetTenantId(commandMock);

        // Assert
        commandMock.CommandText.ShouldBe("SELECT * FROM Users");
    }
}
