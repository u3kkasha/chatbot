using System;
using Chatbot.Api.Infrastructure.MultiTenancy;
using Chatbot.Shared.Models;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Unit.Infrastructure.MultiTenancy;

public class HttpContextTenantProviderTests
{
    private readonly IHttpContextAccessor _httpContextAccessorMock;
    private readonly HttpContextTenantProvider _tenantProvider;

    public HttpContextTenantProviderTests()
    {
        _httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
        _tenantProvider = new HttpContextTenantProvider(_httpContextAccessorMock);
    }

    [Fact]
    public void GetTenantId_ShouldReturnNull_WhenHttpContextIsNull()
    {
        // Arrange
        _httpContextAccessorMock.HttpContext.Returns((HttpContext)null!);

        // Act
        var result = _tenantProvider.GetTenantId();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetTenantId_ShouldReturnTenantId_WhenHeaderIsPresent()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Tenant-Id"] = tenantId.ToString();
        _httpContextAccessorMock.HttpContext.Returns(context);

        // Act
        var result = _tenantProvider.GetTenantId();

        // Assert
        result.ShouldBe(tenantId);
    }

    [Fact]
    public void GetTenantId_ShouldReturnNull_WhenHeaderIsMissing()
    {
        // Arrange
        var context = new DefaultHttpContext();
        _httpContextAccessorMock.HttpContext.Returns(context);

        // Act
        var result = _tenantProvider.GetTenantId();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetTenantId_ShouldReturnNull_WhenHeaderIsInvalid()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Tenant-Id"] = "invalid-guid";
        _httpContextAccessorMock.HttpContext.Returns(context);

        // Act
        var result = _tenantProvider.GetTenantId();

        // Assert
        result.ShouldBeNull();
    }
}
