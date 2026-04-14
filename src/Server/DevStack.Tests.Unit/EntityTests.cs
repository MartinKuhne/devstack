using DevStack.Domain.Entities;
using Xunit;

namespace DevStack.Tests.Unit.Entities;

public class EntityTests
{
    [Fact]
    public void Entity_Has_Generated_Id()
    {
        var entity = new TestEntity();
        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    [Fact]
    public void Entity_Equality_Works_By_Id()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity { Id = id };
        var entity2 = new TestEntity { Id = id };

        Assert.True(entity1 == entity2);
        Assert.True(entity1.Equals(entity2));
    }

    [Fact]
    public void Entity_Inequality_Works()
    {
        var entity1 = new TestEntity { Id = Guid.NewGuid() };
        var entity2 = new TestEntity { Id = Guid.NewGuid() };

        Assert.True(entity1 != entity2);
        Assert.False(entity1.Equals(entity2));
    }

    [Fact]
    public void Entity_Equality_Works_With_Null()
    {
        var entity = new TestEntity();

        Assert.False(entity == null);
        Assert.True(entity != null);
        Assert.False((Entity?)null == entity);
        Assert.True((Entity?)null != entity);
        Assert.True((Entity?)null == (Entity?)null);
    }

    private class TestEntity : Entity
    {
    }
}