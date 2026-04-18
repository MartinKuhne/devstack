using DevStack.Api.GraphQL;
using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL;

public class EpicQueryTests : IAsyncLifetime
{
    private DevStackDbContext? _dbContext;
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DevStackDbContext> _options;
    private readonly Query _query;

    public EpicQueryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<DevStackDbContext>()
            .UseSqlite(_connection)
            .Options;
        _query = new Query();
    }

    public async System.Threading.Tasks.Task InitializeAsync()
    {
        _dbContext = new DevStackDbContext(_options);
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();
        await SeedDataAsync();
    }

    private async System.Threading.Tasks.Task SeedDataAsync()
    {
        if (_dbContext is null) return;

        var projectId = Guid.NewGuid();
        _dbContext.Projects.Add(new Project
        {
            Id = projectId,
            Name = "[TestData] Test Project",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var epic1 = new Item
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "[TestData] Epic One",
            Description = "First epic",
            Subtype = ItemSubtype.Epic,
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var epic2 = new Item
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "[TestData] Epic Two",
            Description = "Second epic",
            Subtype = ItemSubtype.Epic,
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow.AddHours(1),
            UpdatedAt = DateTime.UtcNow.AddHours(1)
        };

        var epic3 = new Item
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "[TestData] Testing Epic",
            Description = "Epic about testing",
            Subtype = ItemSubtype.Epic,
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow.AddHours(2),
            UpdatedAt = DateTime.UtcNow.AddHours(2)
        };

        _dbContext.Items.AddRange(epic1, epic2, epic3);
        await _dbContext.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task DisposeAsync()
    {
        if (_dbContext is not null)
        {
            await _dbContext.DisposeAsync();
        }
        _connection.Close();
    }

    [Fact]
    public void GetItemById_Returns_Epic_When_Exists()
    {
        var epic = _dbContext!.Items.First(i => i.Subtype == ItemSubtype.Epic);
        var result = _query.GetItemById(_dbContext, epic.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(epic.Id);
        result.Title.Should().Be(epic.Title);
    }

    [Fact]
    public void GetItemById_Returns_Null_When_Not_Exists()
    {
        var result = _query.GetItemById(_dbContext!, Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public void GetItems_With_Epic_Subtype_Returns_All_Epics_By_Default()
    {
        var result = _query.GetItems(_dbContext!, subtype: [ItemSubtype.Epic]);

        result.Nodes.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public void GetItems_With_Epic_Subtype_Filters_By_ProjectId()
    {
        var result = _query.GetItems(_dbContext!, projectId: _dbContext!.Projects.First().Id, subtype: [ItemSubtype.Epic]);

        result.Nodes.Should().HaveCount(3);
        result.Nodes.Should().OnlyContain(e => e.ProjectId == _dbContext.Projects.First().Id);
    }

    [Fact]
    public void GetItems_With_Epic_Subtype_Supports_Pagination()
    {
        var result = _query.GetItems(_dbContext!, subtype: [ItemSubtype.Epic], first: 2, skip: 0);

        result.Nodes.Should().HaveCount(2);
        result.PageInfo.HasNextPage.Should().BeTrue();
        result.PageInfo.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void GetItems_With_Epic_Subtype_Supports_Skip()
    {
        var result = _query.GetItems(_dbContext!, subtype: [ItemSubtype.Epic], first: 2, skip: 1);

        result.Nodes.Should().HaveCount(2);
        result.PageInfo.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void GetItems_With_Epic_Subtype_Returns_Empty_When_No_Matches()
    {
        var result = _query.GetItems(_dbContext!, projectId: Guid.NewGuid(), subtype: [ItemSubtype.Epic]);

        result.Nodes.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public void GetItems_With_Epic_Subtype_Returns_Epics_OrderedBy_CreatedAt()
    {
        var result = _query.GetItems(_dbContext!, subtype: [ItemSubtype.Epic]);

        result.Nodes.Should().BeInAscendingOrder(e => e.CreatedAt);
    }
}
