using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace TwitterClone.Architecture.Tests;

public class LayerDependencyTests
{
    private const string DomainNs = "TwitterClone.Domain";
    private const string ApplicationNs = "TwitterClone.Application";
    private const string InfrastructureNs = "TwitterClone.Infrastructure";
    private const string ApiNs = "TwitterClone.Api";

    private static Assembly LoadAssemblyByName(string name)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == name)
            ?? Assembly.Load(name);
    }

    [Fact]
    public void Domain_Should_Not_Reference_Any_Other_Project()
    {
        var assembly = LoadAssemblyByName(DomainNs);
        // Domain is empty at this stage — entities arrive in Task 5.
        // NotBeEmpty guard will be added here once Domain has types.
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNs, InfrastructureNs, ApiNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            result.FailingTypeNames is { } names ? string.Join(", ", names) : string.Empty);
    }

    [Fact]
    public void Application_Should_Not_Reference_Infrastructure_Or_Api()
    {
        var assembly = LoadAssemblyByName(ApplicationNs);
        assembly.GetTypes().Should().NotBeEmpty(because: $"{assembly.GetName().Name} must have at least one type");
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNs, ApiNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            result.FailingTypeNames is { } names ? string.Join(", ", names) : string.Empty);
    }

    [Fact]
    public void Infrastructure_Should_Not_Reference_Api()
    {
        var assembly = LoadAssemblyByName(InfrastructureNs);
        assembly.GetTypes().Should().NotBeEmpty(because: $"{assembly.GetName().Name} must have at least one type");
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApiNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            result.FailingTypeNames is { } names ? string.Join(", ", names) : string.Empty);
    }
}
