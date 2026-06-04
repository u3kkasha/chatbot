using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Chatbot.Tests.Architecture;

public class ArchitectureTests
{
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    // We'll need to load these by name or by a known type in the assembly
    private static readonly Assembly IdentityAssembly = Assembly.Load("Chatbot.Modules.Identity");
    private static readonly Assembly ChatAssembly = Assembly.Load("Chatbot.Modules.Chat");
    private static readonly Assembly KnowledgeAssembly = Assembly.Load("Chatbot.Modules.Knowledge");

    [Fact]
    public void Modules_ShouldNotReferenceEachOther()
    {
        // Arrange
        var modules = new[] { IdentityAssembly, ChatAssembly, KnowledgeAssembly };
        var moduleNames = modules.Select(a => a.GetName().Name).ToArray();

        foreach (var module in modules)
        {
            var otherModules = moduleNames.Where(n => n != module.GetName().Name).ToArray();

            var result = Types
                .InAssembly(module)
                .ShouldNot()
                .HaveDependencyOnAny(otherModules)
                .GetResult();

            // Assert
            result
                .IsSuccessful.Should()
                .BeTrue($"Module {module.GetName().Name} should be isolated.");
        }
    }

    [Fact]
    public void Api_ShouldHaveControllers()
    {
        // Arrange
        var result = Types
            .InAssembly(ApiAssembly)
            .That()
            .ResideInNamespace("Chatbot.Api.Controllers")
            .GetTypes();

        // Assert
        result.Should().NotBeEmpty("The API host should have controllers.");
    }

    [Fact]
    public void IdentityModule_ShouldExist()
    {
        // Arrange
        var result = Types
            .InAssembly(IdentityAssembly)
            .That()
            .HaveName("IdentityModule")
            .GetTypes();

        // Assert
        result.Should().NotBeEmpty("The Identity module should have an IdentityModule type.");
    }
}
