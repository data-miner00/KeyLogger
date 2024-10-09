namespace KeyLogger.Core.Extensions;

using System;

/// <summary>
/// Extension methods for <c>short</c> type.
/// </summary>
public static class ShortExtensions
{
    /// <summary>
    /// Checks whether the first bit of the value is flipped.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The indicator for flipped bit.</returns>
    public static bool FirstBitIsTurnedOn(this short value)
    {
        // 0x8000 == 1000 0000 0000 0000
        return Convert.ToBoolean(value & 0x8000);
    }
}
