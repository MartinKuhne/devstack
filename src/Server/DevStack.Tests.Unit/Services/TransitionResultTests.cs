using DevStack.Domain.Services;
using FluentAssertions;
using Xunit;

public class TransitionResultTests
{
    [Fact]
    public void Success_CreatesResult_WithNoErrors()
    {
        var result = TransitionResult<string>.Success("test value");

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be("test value");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Success_CreatesResult_WithUnit()
    {
        var result = TransitionResult<Unit>.Success(Unit.Value);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(Unit.Value);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithErrorsList_CreatesResult_WithErrorMessages()
    {
        var errors = new List<string> { "error 1", "error 2" };
        var result = TransitionResult<string>.Failure(errors);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(2);
        result.Errors[0].Should().Be("error 1");
        result.Errors[1].Should().Be("error 2");
    }

    [Fact]
    public void Failure_WithSingleErrorMessage_CreatesResult_WithErrorMessage()
    {
        var result = TransitionResult<string>.Failure("something went wrong");

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().ContainSingle().Which.Should().Be("something went wrong");
    }

    [Fact]
    public void IsSuccess_IsTrue_WhenNoErrors()
    {
        var result = TransitionResult<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void IsSuccess_IsFalse_WhenErrorsPresent()
    {
        var result = TransitionResult<int>.Failure(["error"]);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void IsFailure_IsFalse_WhenNoErrors()
    {
        var result = TransitionResult<int>.Success(42);

        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void IsFailure_IsTrue_WhenErrorsPresent()
    {
        var result = TransitionResult<int>.Failure(["error"]);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessResult()
    {
        TransitionResult<string> result = "implicit value";

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be("implicit value");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ImplicitConversion_FromInt_CreatesSuccessResult()
    {
        TransitionResult<int> result = 123;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(123);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ImplicitConversion_ValueIsNotEmpty_ForNonNullTypes()
    {
        TransitionResult<string> result = "test";

        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void ImplicitConversion_ValueIsDefault_ForNull()
    {
        TransitionResult<string?> result = null!;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void Errors_ReturnsReadOnlyList()
    {
        var errors = new List<string> { "error 1" };
        var result = TransitionResult<string>.Failure(errors);

        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Success_Value_IsPreserved_ForCustomType()
    {
        var testObj = new TestRecord("test", 5);
        var result = TransitionResult<TestRecord>.Success(testObj);

        result.Value.Name.Should().Be("test");
        result.Value.Count.Should().Be(5);
    }
}

internal record TestRecord(string Name, int Count);
