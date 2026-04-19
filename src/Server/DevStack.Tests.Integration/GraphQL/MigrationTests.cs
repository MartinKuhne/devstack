using DevStack.Persistence;
using FluentAssertions;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL;

public class MigrationTests
{
    [Fact]
    public void DesignTimeDbContextFactory_CanBeInstantiated()
    {
        var factory = new DesignTimeDbContextFactory();
        factory.Should().NotBeNull();
    }
}