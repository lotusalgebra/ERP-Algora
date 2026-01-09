using Algora.Erp.Application.Common.Models;

namespace Algora.Erp.Application.Tests.Models;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSucceededResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithSingleError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = "Something went wrong";

        // Act
        var result = Result.Failure(error);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.Should().Contain(error);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ShouldCreateFailedResult()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2", "Error 3" };

        // Act
        var result = Result.Failure(errors);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Failure_WithEmptyErrors_ShouldCreateFailedResultWithNoErrors()
    {
        // Arrange
        var errors = Array.Empty<string>();

        // Act
        var result = Result.Failure(errors);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }
}

public class ResultGenericTests
{
    [Fact]
    public void Success_WithData_ShouldCreateSucceededResultWithData()
    {
        // Arrange
        var data = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = Result<TestData>.Success(data);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be(data);
        result.Data!.Id.Should().Be(1);
        result.Data.Name.Should().Be("Test");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResultWithNullData()
    {
        // Arrange
        var error = "Not found";

        // Act
        var result = Result<TestData>.Failure(error);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Errors.Should().ContainSingle();
        result.Errors.Should().Contain(error);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ShouldCreateFailedResultWithNullData()
    {
        // Arrange
        var errors = new[] { "Validation error 1", "Validation error 2" };

        // Act
        var result = Result<TestData>.Failure(errors);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Success_WithString_ShouldReturnStringData()
    {
        // Arrange
        var message = "Operation completed successfully";

        // Act
        var result = Result<string>.Success(message);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be(message);
    }

    [Fact]
    public void Success_WithInt_ShouldReturnIntData()
    {
        // Arrange
        var count = 42;

        // Act
        var result = Result<int>.Success(count);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be(42);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
