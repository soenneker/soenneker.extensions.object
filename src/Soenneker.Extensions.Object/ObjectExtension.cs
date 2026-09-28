using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Soenneker.Extensions.Type;

namespace Soenneker.Extensions.Object;

/// <summary>
/// A collection of helpful Object extension methods.
/// </summary>
public static partial class ObjectExtension
{
    /// <summary>
    /// Determines whether the specified object is of a numeric type.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <returns><c>true</c> if the object is of a numeric type; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="obj"/> is null.</exception>
    [Pure]
    public static bool IsObjectNumeric(this object obj)
    {
        return obj.GetType()
                  .IsNumeric();
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the input object is null.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="name">The name of the calling member.</param>
    /// <exception cref="ArgumentNullException">Thrown when the input object is null.</exception>
    public static void ThrowIfNull([NotNull] this object? input, [CallerMemberName] string? name = null)
    {
        ArgumentNullException.ThrowIfNull(input, name);
    }
}
