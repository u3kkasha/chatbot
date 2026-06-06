using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
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
            result.IsSuccessful.ShouldBeTrue($"Module {module.GetName().Name} should be isolated.");
        }
    }

    [Fact]
    public void Api_ShouldNotHaveControllers()
    {
        // Arrange
        var result = Types
            .InAssembly(ApiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .GetTypes();

        // Assert
        result.ShouldBeEmpty(
            "The API host should not use reflection-based MVC controllers to maintain Native AOT compatibility."
        );
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
        result.ShouldNotBeEmpty("The Identity module should have an IdentityModule type.");
    }

    [Fact]
    public void Assemblies_ShouldNotHaveDependenciesOnAutoMapperOrMapster()
    {
        // Arrange
        var assemblies = new[] { IdentityAssembly, ChatAssembly, KnowledgeAssembly, ApiAssembly };

        foreach (var assembly in assemblies)
        {
            var result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny("AutoMapper", "Mapster")
                .GetResult();

            // Assert
            result.IsSuccessful.ShouldBeTrue(
                $"Assembly {assembly.GetName().Name} should not depend on AutoMapper or Mapster to enforce Manual Mapping Only."
            );
        }
    }
}
