namespace KeyLogger.Core.UnitTests.Extensions;

using KeyLogger.Core.Extensions;
using Xunit;

public sealed class ShortExtensionsTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(-32768)]
    public void FirstBitIsTurnedOn_FirstBitTurnedOn_ReturnTrue(short value)
    {
        var result = value.FirstBitIsTurnedOn();
        Assert.True(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(32767)]
    public void FirstBitIsTurnedOn_FirstBitTurnedOff_ReturnFalse(short value)
    {
        var result = value.FirstBitIsTurnedOn();
        Assert.False(result);
    }
}
