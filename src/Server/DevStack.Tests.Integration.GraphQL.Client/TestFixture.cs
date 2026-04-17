using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using DevStack.Infrastructure.Projects;
using DevStack.Infrastructure.Features;
using DevStack.Infrastructure.Defects;
using DevStack.Infrastructure.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL.Client;

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
}

public class IntegrationTestFixture : IAsyncLifetime
{
    private DevStackDbContext? _dbContext;
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DevStackDbContext> _options;

    public IntegrationTestFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<DevStackDbContext>()
            .UseSqlite(_connection)
            .Options;
        
        _dbContext = new DevStackDbContext(_options);
    }

    public DevStackDbContext CreateDbContext()
    {
        return new DevStackDbContext(_options);
    }

    public async Task InitializeAsync()
    {
        await _dbContext!.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dbContext is not null)
        {
            await _dbContext.DisposeAsync();
        }
        _connection.Close();
    }

    public async Task<Guid> CreateTestProjectAsync(string name, string? description = null)
    {
        var mutation = new DevStack.Api.GraphQL.Types.Mutation();
        var input = new DevStack.Api.GraphQL.Types.CreateProjectInput(name, description, null, null, null);
        var handler = new DevStack.Infrastructure.Projects.CreateProjectHandler(CreateDbContext());
        
        var result = await mutation.CreateProjectAsync(input, handler, CancellationToken.None);
        
        result.Errors.Should().BeEmpty();
        return result.Project!.Id;
    }

    public async Task<Guid> CreateTestFeatureAsync(Guid projectId, string title, string? description = null)
    {
        var mutation = new DevStack.Api.GraphQL.Types.Mutation();
        var input = new DevStack.Api.GraphQL.Types.CreateFeatureInput(projectId, title, description, null, null, null, null, null, null, null, null);
        var handler = new DevStack.Infrastructure.Features.CreateFeatureHandler(CreateDbContext());
        
        var result = await mutation.CreateFeatureAsync(input, handler, CancellationToken.None);
        
        result.Errors.Should().BeEmpty();
        return result.Item!.Id;
    }

    public async Task<Guid> CreateTestDefectAsync(Guid projectId, Guid? parentFeatureId, string title, Severity? severity = null)
    {
        var mutation = new DevStack.Api.GraphQL.Types.Mutation();
        var input = new DevStack.Api.GraphQL.Types.CreateDefectInput(projectId, parentFeatureId, title, null, null, null, null, null, null, null, severity, null);
        var handler = new DevStack.Infrastructure.Defects.CreateDefectHandler(CreateDbContext());
        
        var result = await mutation.CreateDefectAsync(input, handler, CancellationToken.None);
        
        result.Errors.Should().BeEmpty();
        return result.Item!.Id;
    }

    public async Task<Guid> CreateTestTaskAsync(Guid projectId, Guid itemId, string title, int complexityRating)
    {
        var mutation = new DevStack.Api.GraphQL.Types.Mutation();
        var input = new DevStack.Api.GraphQL.Types.CreateTaskInput(projectId, itemId, title, null, null, null, null, null, complexityRating);
        var handler = new DevStack.Infrastructure.Tasks.CreateTaskHandler(CreateDbContext());
        
        var result = await mutation.CreateTaskAsync(input, handler, CancellationToken.None);
        
        result.Errors.Should().BeEmpty();
        return result.Task!.Id;
    }

    public async Task UpdateFeatureStatusAsync(Guid featureId, FeatureStatus targetStatus, string actor)
    {
        var mutation = new DevStack.Api.GraphQL.Types.Mutation();
        var input = new DevStack.Api.GraphQL.Types.TransitionFeatureInput(featureId, targetStatus, actor);
        var handler = new DevStack.Infrastructure.Features.TransitionFeatureStatusHandler(CreateDbContext(), new DevStack.Domain.Services.ItemStatusTransitionService());
        
        var result = await mutation.TransitionFeatureStatusAsync(input, handler, CancellationToken.None);
        
        result.Errors.Should().BeEmpty();
    }

    public async Task UpdateTaskStatusAsync(Guid taskId, Domain.Enums.TaskStatus targetStatus, string actor)
    {
        var mutation = new DevStack.Api.GraphQL.Types.Mutation();
        var input = new DevStack.Api.GraphQL.Types.TransitionTaskInput(taskId, targetStatus, actor);
        var handler = new DevStack.Infrastructure.Tasks.TransitionTaskStatusHandler(CreateDbContext(), new DevStack.Domain.Services.ItemStatusTransitionService());
        
        var result = await mutation.TransitionTaskStatusAsync(input, handler, CancellationToken.None);
        
        result.Errors.Should().BeEmpty();
    }

    public async Task UpdateDefectStatusAsync(Guid defectId, FeatureStatus targetStatus, string actor)
    {
        var mutation = new DevStack.Api.GraphQL.Types.Mutation();
        var input = new DevStack.Api.GraphQL.Types.TransitionDefectInput(defectId, targetStatus, actor);
        var handler = new DevStack.Infrastructure.Defects.TransitionDefectStatusHandler(CreateDbContext(), new DevStack.Domain.Services.ItemStatusTransitionService());
        
        var result = await mutation.TransitionDefectStatusAsync(input, handler, CancellationToken.None);
        
        result.Errors.Should().BeEmpty();
    }
}