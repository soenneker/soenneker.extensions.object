using System;
using System.Buffers;
using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Soenneker.Utils.Json;
using Soenneker.Utils.PooledStringBuilders;

namespace Soenneker.Extensions.Object;

public static partial class ObjectExtension
{
    /// <summary>
    /// Builds a query string using caller-supplied JSON metadata without discovering properties through reflection.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="typeInfo">Source-generated JSON metadata and serialization options for the object.</param>
    /// <returns>An escaped query string beginning with '?', or an empty string for null or an object with no serialized properties.</returns>
    /// <remarks>
    /// Honors the supplied metadata's property names, ignore conditions, and converters.
    /// Strings use their decoded values, null values become empty values, and other values use JSON formatting.
    /// Nested objects and arrays are encoded as JSON values rather than flattened.
    /// Use source-generated metadata and reflection-free converters for trimming and Native AOT compatibility.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The metadata is null.</exception>
    /// <exception cref="ArgumentException">The non-null value does not serialize as a JSON object.</exception>
    [Pure]
    public static string ToQueryString<T>(this T obj, JsonTypeInfo<T> typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);

        if (obj is null)
            return string.Empty;

        JsonElement element = JsonUtil.SerializeToElement(obj, typeInfo);

        if (element.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("The value must serialize as a JSON object.", nameof(obj));

        Span<char> initialBuffer = stackalloc char[256];
        Span<char> escapeBuffer = stackalloc char[256];

        var builder = new PooledStringBuilder(initialBuffer);

        try
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                builder.Append(builder.Length == 0 ? '?' : '&');

                AppendEscaped(ref builder, property.Name, escapeBuffer);
                builder.Append('=');

                JsonElement value = property.Value;

                switch (value.ValueKind)
                {
                    case JsonValueKind.Null:
                        break;

                    case JsonValueKind.True:
                        builder.Append("true");
                        break;

                    case JsonValueKind.False:
                        builder.Append("false");
                        break;

                    case JsonValueKind.String:
                        AppendEscaped(ref builder, value.GetString(), escapeBuffer);
                        break;

                    default:
                        AppendEscaped(ref builder, value.GetRawText(), escapeBuffer);
                        break;
                }
            }

            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }

    private static void AppendEscaped(ref PooledStringBuilder builder, string? value, Span<char> scratch)
    {
        if (string.IsNullOrEmpty(value))
            return;

        if (Uri.TryEscapeDataString(value.AsSpan(), scratch, out int written))
        {
            builder.Append(scratch[..written]);
            return;
        }

        // At most 3 UTF-8 bytes per UTF-16 code unit, each escaped as %XX.
        int capacity = checked(value.Length * 9);
        char[] rented = ArrayPool<char>.Shared.Rent(capacity);

        try
        {
            if (!Uri.TryEscapeDataString(value.AsSpan(), rented.AsSpan(), out written))
            {
                throw new InvalidOperationException("The URI escaping buffer was too small.");
            }

            builder.Append(rented.AsSpan(0, written));
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rented);
        }
    }
}