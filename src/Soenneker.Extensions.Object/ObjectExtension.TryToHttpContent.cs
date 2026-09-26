using System.Text.Json.Serialization.Metadata;
using System;
using System.Diagnostics.Contracts;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Soenneker.Extensions.Object;

/// <summary>
/// Represents the object extension.
/// </summary>
public static partial class ObjectExtension
{
    /// <summary>
    /// Attempts to convert an object to an <see cref="HttpContent"/> with JSON content,
    /// logging any serialization errors.
    /// </summary>
    /// <param name="obj">The object to serialize into JSON content.</param>
    /// <param name="logger">Optional <see cref="ILogger"/> for logging serialization errors.</param>
    /// <returns>
    /// An <see cref="HttpContent"/> containing the JSON representation of the object,
    /// or <c>null</c> if serialization fails.
    /// </returns>
    /// <param name="typeInfo">Source-generated JSON metadata and serialization options for the value.</param>
    [Pure]
    public static HttpContent? TryToHttpContent<T>(this T obj, JsonTypeInfo<T> typeInfo, ILogger? logger = null)
    {
        try
        {
            return ToHttpContent(obj, typeInfo);
        } 
        catch (JsonException ex)
        {
            logger?.LogError(ex, "Failed to serialize object to HttpContent for type ({type})", obj?.GetType()
                .Name);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while converting object to HttpContent for type ({type}): {Message}", obj?.GetType()
                .Name, ex.Message);
        }

        return null;
    }

    /// <summary>
    /// Attempts to serialize an object to JSON and construct an <see cref="HttpContent"/> with the serialized string.
    /// Logs any serialization errors.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="logger">Optional <see cref="ILogger"/> for logging serialization errors.</param>
    /// <returns>
    /// A tuple containing an <see cref="HttpContent"/> and the serialized JSON string.
    /// If serialization fails, returns <c>null</c> for both values.
    /// </returns>
    /// <param name="typeInfo">Source-generated JSON metadata and serialization options for the value.</param>
    [Pure]
    public static (HttpContent? httpContent, string? str) TryToHttpContentAndString<T>(this T obj, JsonTypeInfo<T> typeInfo, ILogger? logger = null)
    {
        try
        {
            return ToHttpContentAndString(obj, typeInfo);
        }
        catch (JsonException ex)
        {
            logger?.LogError(ex, "Failed to serialize object to HttpContent for type ({type})", obj?.GetType()
                .Name);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while converting object to HttpContent for type ({type}): {Message}", obj?.GetType()
                .Name, ex.Message);
        }

        return (null, null);
    }

    /// <summary>
    /// Attempts to serialize an object to JSON, construct an <see cref="HttpContent"/>, and add the 'x-api-key' header.
    /// Logs any serialization or header addition errors.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="apiKey">The API key to add as a header.</param>
    /// <param name="logger">Optional <see cref="ILogger"/> for logging errors.</param>
    /// <returns> 
    /// An <see cref="HttpContent"/> with the 'x-api-key' header added, or <c>null</c> if an error occurs.
    /// </returns>
    /// <param name="typeInfo">Source-generated JSON metadata and serialization options for the value.</param>
    [Pure]
    public static HttpContent? TryToHttpContentWithKey<T>(this T obj, JsonTypeInfo<T> typeInfo, string apiKey, ILogger? logger = null)
    {
        try
        {
            return ToHttpContentWithKey(obj, typeInfo, apiKey);
        }
        catch (JsonException ex)
        {
            logger?.LogError(ex, "Failed to serialize object to HttpContent for type ({type})", obj?.GetType()
                .Name);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while converting object to HttpContent for type ({type}): {Message}", obj?.GetType()
                .Name, ex.Message);
        }

        return null;
    }
}
