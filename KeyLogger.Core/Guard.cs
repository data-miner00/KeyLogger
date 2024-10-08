﻿namespace KeyLogger.Core;

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>
/// A utility class that provides argument checking.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Checks that throws <see cref="ArgumentNullException"/> when the provided object is null.
    /// </summary>
    /// <typeparam name="T">Any class object.</typeparam>
    /// <param name="obj">The object to be checked.</param>
    /// <param name="paramName">The name of the object.</param>
    /// <returns>The object itself.</returns>
    /// <exception cref="ArgumentException">Throws when .</exception>
    public static T ThrowIfNull<T>(T? obj, [CallerArgumentExpression(nameof(obj))] string? paramName = null)
        where T : class
    {
        if (obj is null)
        {
            throw new ArgumentNullException(paramName, string.Create(CultureInfo.InvariantCulture, $"The object {paramName} cannot be null."));
        }

        return obj;
    }

    /// <summary>
    /// Checks that throws <see cref="ArgumentException"/> when the provided string is null or empty.
    /// </summary>
    /// <param name="str">The string to be checked.</param>
    /// <param name="paramName">The name of the string.</param>
    /// <returns>The string itself.</returns>
    /// <exception cref="ArgumentException">Throws when string is null or empty.</exception>
    public static string ThrowIfNullOrWhitespace(string? str, [CallerArgumentExpression(nameof(str))] string? paramName = null)
    {
        if (string.IsNullOrEmpty(str))
        {
            throw new ArgumentException(paramName, string.Create(CultureInfo.InvariantCulture, $"The string {paramName} cannot be null or whitespace"));
        }

        return str;
    }

    /// <summary>
    /// Checks that throws <see cref="ValidationException"/> when the attribute failed to comply to the rules.
    /// </summary>
    /// <typeparam name="T">Any class object.</typeparam>
    /// <param name="obj">The object to be checked.</param>
    /// <returns>The validated object.</returns>
    public static T ThrowIfValidationFailed<T>(T obj)
        where T : class
    {
        var context = new ValidationContext(obj);
        Validator.ValidateObject(obj, context, true);

        return obj;
    }

    /// <summary>
    /// Throws if the value type element is at default value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The item to validate.</param>
    /// <param name="paramName">The parameter name for reference.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentException">Throws when value is default.</exception>
    public static T ThrowIfDefault<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : struct
    {
        if (default(T).Equals(value))
        {
            throw new ArgumentException(string.Create(CultureInfo.InvariantCulture, $"Argument '{paramName}' cannot be default value."));
        }

        return value;
    }

    /// <summary>
    /// Throws if the numeric value is less than the specified threshold.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="threshold">The threshold value.</param>
    /// <param name="paramName">The parameter name for reference.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentException">The argument exception.</exception>
    public static T ThrowIfLessThan<T>(T value, T threshold, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : INumber<T>
    {
        if (value < threshold)
        {
            throw new ArgumentException(string.Create(CultureInfo.InvariantCulture, $"Argument '{paramName}' cannot be less than {threshold}."));
        }

        return value;
    }

    /// <summary>
    /// Throws if the numeric value is more than the specified threshold.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="threshold">The threshold value.</param>
    /// <param name="paramName">The parameter name for reference.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentException">The argument exception.</exception>
    public static T ThrowIfMoreThan<T>(T value, T threshold, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : INumber<T>
    {
        if (value > threshold)
        {
            throw new ArgumentException(string.Create(CultureInfo.InvariantCulture, $"Argument '{paramName}' cannot be more than {threshold}."));
        }

        return value;
    }
}
