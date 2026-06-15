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
        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == name);
        if (loaded is not null)
        {
            return loaded;
        }

        var dir = Path.GetDirectoryName(typeof(LayerDependencyTests).Assembly.Location)!;
        var path = Path.Combine(dir, $"{name}.dll");
        return Assembly.LoadFrom(path);
    }

    [Fact]
    public void Domain_Should_Not_Reference_Any_Other_Project()
    {
        var assembly = LoadAssemblyByName(DomainNs);
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
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            result.FailingTypeNames is { } names ? string.Join(", ", names) : string.Empty);
    }
}
