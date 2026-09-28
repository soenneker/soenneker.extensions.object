using System;
using System.Diagnostics.Contracts;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Soenneker.Utils.Json;

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

        using JsonDocument document = JsonDocument.Parse(JsonUtil.SerializeToUtf8Bytes(obj, typeInfo));
        JsonElement element = document.RootElement;
        if (element.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("The value must serialize as a JSON object.", nameof(obj));

        var builder = new StringBuilder();
        foreach (JsonProperty property in element.EnumerateObject())
        {
            builder.Append(builder.Length == 0 ? '?' : '&');
            builder.Append(Uri.EscapeDataString(property.Name));
            builder.Append('=');

            string value = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString()!,
                JsonValueKind.Null => string.Empty,
                _ => property.Value.GetRawText()
            };
            builder.Append(Uri.EscapeDataString(value));
        }

        return builder.ToString();
    }
}
