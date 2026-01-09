using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Tests.Entities;

public class BaseEntityTests
{
    private class TestEntity : BaseEntity { }

    [Fact]
    public void NewEntity_ShouldHaveUniqueId()
    {
        // Arrange & Act
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Assert
        entity1.Id.Should().NotBe(Guid.Empty);
        entity2.Id.Should().NotBe(Guid.Empty);
        entity1.Id.Should().NotBe(entity2.Id);
    }

    [Fact]
    public void AddDomainEvent_ShouldAddEventToCollection()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new { Name = "TestEvent" };

        // Act
        entity.AddDomainEvent(domainEvent);

        // Assert
        entity.DomainEvents.Should().ContainSingle();
        entity.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void RemoveDomainEvent_ShouldRemoveEventFromCollection()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new { Name = "TestEvent" };
        entity.AddDomainEvent(domainEvent);

        // Act
        entity.RemoveDomainEvent(domainEvent);

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var entity = new TestEntity();
        entity.AddDomainEvent(new { Name = "Event1" });
        entity.AddDomainEvent(new { Name = "Event2" });
        entity.AddDomainEvent(new { Name = "Event3" });

        // Act
        entity.ClearDomainEvents();

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_ShouldBeReadOnly()
    {
        // Arrange
        var entity = new TestEntity();

        // Act & Assert
        entity.DomainEvents.Should().BeAssignableTo<IReadOnlyCollection<object>>();
    }
}
