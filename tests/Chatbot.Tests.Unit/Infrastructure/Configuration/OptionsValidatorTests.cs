using Chatbot.Shared.Infrastructure.Configuration;
using Shouldly;
using Xunit;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Chatbot.Tests.Unit.Infrastructure.Configuration;

public class OptionsValidatorTests
{
    [Fact]
    public void ConnectionStringsOptionsValidator_ShouldFail_WhenRequiredPropertiesAreMissing()
    {
        // given
        var validator = new ConnectionStringsOptionsValidator();
        var options = new ConnectionStringsOptions();

        // when
        var result = validator.Validate(null, options);

        // then
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("DefaultConnection");
        result.FailureMessage.ShouldContain("Redis");
        result.FailureMessage.ShouldContain("BlobStorage");
    }

    [Fact]
    public void ConnectionStringsOptionsValidator_ShouldSucceed_WhenAllPropertiesAreProvided()
    {
        // given
        var validator = new ConnectionStringsOptionsValidator();
        var options = new ConnectionStringsOptions
        {
            DefaultConnection = "Host=localhost;Database=test",
            Redis = "localhost:6379",
            BlobStorage = "UseDevelopmentStorage=true"
        };

        // when
        var result = validator.Validate(null, options);

        // then
        result.Failed.ShouldBeFalse();
    }

    [Fact]
    public void AiOptionsValidator_ShouldFail_WhenRequiredPropertiesAreMissing()
    {
        // given
        var validator = new AiOptionsValidator();
        var options = new AiOptions();

        // when
        var result = validator.Validate(null, options);

        // then
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("Endpoint");
        result.FailureMessage.ShouldContain("ApiKey");
        result.FailureMessage.ShouldContain("ModelId");
    }

    [Fact]
    public void AiOptionsValidator_ShouldSucceed_WhenAllPropertiesAreProvided()
    {
        // given
        var validator = new AiOptionsValidator();
        var options = new AiOptions
        {
            Endpoint = "https://api.openai.com/v1",
            ApiKey = "some-key",
            ModelId = "gpt-4"
        };

        // when
        var result = validator.Validate(null, options);

        // then
        result.Failed.ShouldBeFalse();
    }

    [Fact]
    public void ProcessingOptionsValidator_ShouldFail_WhenRequiredPropertiesAreMissing()
    {
        // given
        var validator = new ProcessingOptionsValidator();
        var options = new ProcessingOptions();

        // when
        var result = validator.Validate(null, options);

        // then
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("BaseUrl");
    }

    [Fact]
    public void ProcessingOptionsValidator_ShouldSucceed_WhenAllPropertiesAreProvided()
    {
        // given
        var validator = new ProcessingOptionsValidator();
        var options = new ProcessingOptions
        {
            BaseUrl = "http://localhost:5000"
        };

        // when
        var result = validator.Validate(null, options);

        // then
        result.Failed.ShouldBeFalse();
    }

    [Fact]
    public void QdrantOptionsValidator_ShouldFail_WhenPortIsInvalid()
    {
        // given
        var validator = new QdrantOptionsValidator();
        var options = new QdrantOptions
        {
            Host = "localhost",
            Port = 0 // Invalid port (Range 1-65535)
        };

        // when
        var result = validator.Validate(null, options);

        // then
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("Port");
    }

    [Fact]
    public void QdrantOptionsValidator_ShouldSucceed_WhenDefaultValuesAreUsed()
    {
        // given
        var validator = new QdrantOptionsValidator();
        var options = new QdrantOptions();

        // when
        var result = validator.Validate(null, options);

        // then
        result.Failed.ShouldBeFalse();
    }

    [Fact]
    public void AiOptions_ShouldBindFromConfigurationSection()
    {
        // given
        var inMemorySettings = new Dictionary<string, string>
        {
            { "AI:Endpoint", "https://api.openai.com/v1" },
            { "AI:ModelId", "gpt-4o" },
            { "AI:ApiKey", "expected-api-key" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();

        // when
        var aiBuilder = services
            .AddOptions<AiOptions>()
            .Bind(configuration.GetSection(AiOptions.SectionName));

        services.AddSingleton<IValidateOptions<AiOptions>, AiOptionsValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<AiOptions>>().Value;

        // then
        options.ApiKey.ShouldBe("expected-api-key");
    }
}

