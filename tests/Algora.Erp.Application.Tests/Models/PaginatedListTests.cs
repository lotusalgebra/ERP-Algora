using Algora.Erp.Application.Common.Models;

namespace Algora.Erp.Application.Tests.Models;

public class PaginatedListTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2", "Item3" };
        var totalCount = 10;
        var pageNumber = 1;
        var pageSize = 3;

        // Act
        var paginatedList = new PaginatedList<string>(items, totalCount, pageNumber, pageSize);

        // Assert
        paginatedList.Items.Should().BeEquivalentTo(items);
        paginatedList.TotalCount.Should().Be(totalCount);
        paginatedList.PageNumber.Should().Be(pageNumber);
        paginatedList.PageSize.Should().Be(pageSize);
        paginatedList.TotalPages.Should().Be(4); // 10 items / 3 per page = 4 pages (rounded up)
    }

    [Fact]
    public void TotalPages_ShouldCalculateCorrectly()
    {
        // Arrange & Act
        var list1 = new PaginatedList<int>(new List<int>(), 10, 1, 3);
        var list2 = new PaginatedList<int>(new List<int>(), 9, 1, 3);
        var list3 = new PaginatedList<int>(new List<int>(), 1, 1, 10);
        var list4 = new PaginatedList<int>(new List<int>(), 0, 1, 10);

        // Assert
        list1.TotalPages.Should().Be(4); // 10/3 = 3.33 -> 4
        list2.TotalPages.Should().Be(3); // 9/3 = 3
        list3.TotalPages.Should().Be(1); // 1/10 = 0.1 -> 1
        list4.TotalPages.Should().Be(0); // 0/10 = 0
    }

    [Theory]
    [InlineData(1, 10, false)] // First page, no previous
    [InlineData(2, 10, true)]  // Second page, has previous
    [InlineData(5, 10, true)]  // Middle page, has previous
    [InlineData(10, 10, true)] // Last page, has previous
    public void HasPreviousPage_ShouldBeCorrect(int pageNumber, int totalPages, bool expected)
    {
        // Arrange
        var totalCount = totalPages * 10; // 10 items per page
        var list = new PaginatedList<int>(new List<int>(), totalCount, pageNumber, 10);

        // Act & Assert
        list.HasPreviousPage.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 10, true)]   // First page of 10, has next
    [InlineData(9, 10, true)]   // 9th page of 10, has next
    [InlineData(10, 10, false)] // Last page, no next
    [InlineData(1, 1, false)]   // Only page, no next
    public void HasNextPage_ShouldBeCorrect(int pageNumber, int totalPages, bool expected)
    {
        // Arrange
        var totalCount = totalPages * 10; // 10 items per page
        var list = new PaginatedList<int>(new List<int>(), totalCount, pageNumber, 10);

        // Act & Assert
        list.HasNextPage.Should().Be(expected);
    }

    [Fact]
    public void EmptyList_ShouldHandleCorrectly()
    {
        // Arrange & Act
        var list = new PaginatedList<string>(new List<string>(), 0, 1, 10);

        // Assert
        list.Items.Should().BeEmpty();
        list.TotalCount.Should().Be(0);
        list.TotalPages.Should().Be(0);
        list.HasPreviousPage.Should().BeFalse();
        list.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void SingleItem_ShouldHandleCorrectly()
    {
        // Arrange
        var items = new List<int> { 42 };

        // Act
        var list = new PaginatedList<int>(items, 1, 1, 10);

        // Assert
        list.Items.Should().ContainSingle();
        list.Items[0].Should().Be(42);
        list.TotalCount.Should().Be(1);
        list.TotalPages.Should().Be(1);
        list.HasPreviousPage.Should().BeFalse();
        list.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void LargeDataset_ShouldCalculatePagesCorrectly()
    {
        // Arrange
        var items = Enumerable.Range(1, 25).ToList();
        var totalCount = 1000;
        var pageNumber = 5;
        var pageSize = 25;

        // Act
        var list = new PaginatedList<int>(items, totalCount, pageNumber, pageSize);

        // Assert
        list.Items.Should().HaveCount(25);
        list.TotalCount.Should().Be(1000);
        list.TotalPages.Should().Be(40); // 1000/25 = 40
        list.HasPreviousPage.Should().BeTrue();
        list.HasNextPage.Should().BeTrue();
    }
}
