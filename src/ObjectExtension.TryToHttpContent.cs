using System;
using System.Diagnostics.Contracts;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Soenneker.Extensions.Object;

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
    [Pure]
    public static HttpContent? TryToHttpContent(this object? obj, ILogger? logger = null)
    {
        try
        {
            return ToHttpContent(obj);
        }
        catch (JsonSerializationException ex)
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
    [Pure]
    public static (HttpContent? httpContent, string? str) TryToHttpContentAndString(this object? obj, ILogger? logger = null)
    {
        try
        {
            return ToHttpContentAndString(obj);
        }
        catch (JsonSerializationException ex)
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
    [Pure]
    public static HttpContent? TryToHttpContentWithKey(this object? obj, string apiKey, ILogger? logger = null)
    {
        try
        {
            return ToHttpContentWithKey(obj, apiKey);
        }
        catch (JsonSerializationException ex)
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