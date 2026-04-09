namespace Fiap.FCG.Tests;

public class EntityTests
{
    [Fact]
    public void Entity_ShouldHaveNonEmptyIdOnCreation()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    [Fact]
    public void Entity_ShouldSetCreatedAtOnCreation()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var entity = new TestEntity();

        // Assert
        Assert.True(entity.CreatedAt >= before);
        Assert.Null(entity.UpdatedAt);
    }

    private class TestEntity : Fiap.FCG.Domain.Entities.Entity
    {
        public void Touch() => SetUpdatedAt();
    }
}
