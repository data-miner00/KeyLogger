namespace KeyLogger.Core.UnitTests;

using System;
using System.ComponentModel.DataAnnotations;
using KeyLogger.Core;
using Xunit;

public sealed class GuardTests
{
    internal sealed class TestSubject
    {
        [Required]
        [Range(0, 5)]
        public int TestProperty { get; set; }
    }

    [Fact]
    public void ThrowIfNull_NullParameter_Throws()
    {
        object? mockObj = null;
        Assert.Throws<ArgumentNullException>(() => Guard.ThrowIfNull(mockObj));
    }

    [Fact]
    public void ThrowIfNull_ValidParameter_Pass()
    {
        object mockObj = new();
        var result = Guard.ThrowIfNull(mockObj);
        Assert.Equal(result, mockObj);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ThrowIfNullOrWhitespace_NullOrEmptyString_Throws(string param)
    {
        Assert.Throws<ArgumentException>(() => Guard.ThrowIfNullOrWhitespace(param));
    }

    [Fact]
    public void ThrowIfValidationFailed_BadAttribute_Throws()
    {
        var testObj = new TestSubject { TestProperty = 42, };
        Assert.Throws<ValidationException>(() => Guard.ThrowIfValidationFailed(testObj));
    }

    [Fact]
    public void ThrowIfValidationFailed_ValidAttribute_Pass()
    {
        var testObj = new TestSubject { TestProperty = 3, };
        var result = Guard.ThrowIfValidationFailed(testObj);
        Assert.Equal(result, testObj);
    }

    [Fact]
    public void ThrowIfDefault_DefaultValue_Throws()
    {
        var defaultGuid = Guid.Empty;
        Assert.Throws<ArgumentException>(() => Guard.ThrowIfDefault(defaultGuid));
    }

    [Fact]
    public void ThrowIfMoreThan_LessThanThreshold_Pass()
    {
        var value = 5;
        var threshold = 6;

        var result = Guard.ThrowIfMoreThan(value, threshold);
        Assert.Equal(value, result);
    }

    [Fact]
    public void ThrowIfMoreThan_MoreThanThreshold_Throws()
    {
        var value = 5;
        var threshold = 4;

        Assert.Throws<ArgumentException>(() => Guard.ThrowIfMoreThan(value, threshold));
    }

    [Fact]
    public void ThrowIfLessThan_MoreThanThreshold_Pass()
    {
        var value = 6;
        var threshold = 5;

        var result = Guard.ThrowIfLessThan(value, threshold);
        Assert.Equal(value, result);
    }

    [Fact]
    public void ThrowIfLessThan_LessThanThreshold_Throws()
    {
        var value = 4;
        var threshold = 5;

        Assert.Throws<ArgumentException>(() => Guard.ThrowIfLessThan(value, threshold));
    }
}
