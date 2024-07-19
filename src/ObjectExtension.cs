using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Soenneker.Constants.Auth;
using Soenneker.Extensions.String;
using Soenneker.Extensions.Type;
using Soenneker.Utils.Json;
using Soenneker.Utils.Json.Abstract;

namespace Soenneker.Extensions.Object;

/// <summary>
/// A collection of helpful Object extension methods
/// </summary>
public static class ObjectExtension
{
    /// <returns>An application/json HttpContent, with JsonUtil.WebOptions. <para/>
    /// If the object is null, returns a new HttpContent with empty content.
    /// </returns>
    /// <remarks>Will not log result, see <see cref="IJsonUtil"/> for that</remarks>
    [Pure]
    public static HttpContent ToHttpContent(this object? obj)
    {
        string? jsonContent = obj != null ? JsonUtil.Serialize(obj) : "";

        var result = new StringContent(jsonContent!, Encoding.UTF8, MediaTypeNames.Application.Json);
        return result;
    }

    /// <returns>An application/json HttpContent, with JsonUtil.WebOptions. <para/>
    /// If the object is null, returns a new HttpContent with empty content. <para/>
    /// Also returns the serialized string that was used to construct the HttpContent.
    /// </returns>
    [Pure]
    public static (HttpContent httpContent, string str) ToHttpContentAndString(this object? obj)
    {
        string? jsonContent = obj != null ? JsonUtil.Serialize(obj) : "";

        var content = new StringContent(jsonContent!, Encoding.UTF8, MediaTypeNames.Application.Json);
        return (content, jsonContent!);
    }

    /// <summary>
    /// <see cref="ToHttpContent"/> and then adds the 'x-api-key' header to the request
    /// </summary>
    [Pure]
    public static HttpContent ToHttpContentWithKey(this object? obj, string apiKey)
    {
        var httpContent = obj.ToHttpContent();
        httpContent.Headers.TryAddWithoutValidation(AuthConstants.XApiKey, apiKey);

        return httpContent;
    }

    [Pure]
    public static bool IsObjectNumeric(this object obj)
    {
        return obj.GetType().IsNumeric();
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the input object is null.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="name">The name of the calling member.</param>
    /// <exception cref="ArgumentNullException">Thrown when the input object is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNull([NotNull] this object? input, [CallerMemberName] string? name = null)
    {
        if (input == null)
            throw new ArgumentNullException(name);
    }

    [Pure]
    public static IDictionary<string, object?> ToDictionary(this object source)
    {
        const BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

        System.Type type = source.GetType();
        PropertyInfo[] properties = type.GetProperties(bindingAttr);

        Dictionary<string, object?> dictionary = properties.ToDictionary
        (
            propInfo => propInfo.Name,
            propInfo => propInfo.GetValue(source, null)
        );

        return dictionary;
    }

    /// <summary>
    /// Uses Reflection to build a query string out of an object. If object is null, returns an empty string. Uses the object's property names as the keys of the query string.
    /// </summary>
    /// <remarks>This string's first character is a question mark (unless the object is null, then it's null)</remarks>
    [Pure]
    public static string ToQueryStringViaReflection(this object? obj, bool loweredPropertyNames = true)
    {
        if (obj == null)
            return "";

        System.Type type = obj.GetType();
        PropertyInfo[] properties = type.GetProperties().Where(prop => prop.CanRead).ToArray();

        if (properties.Length == 0)
            return "";

        var queryString = new StringBuilder(properties.Length * 10);

        var firstParameterAdded = false;
        foreach (PropertyInfo property in properties)
        {
            object? value = property.GetValue(obj);
            if (value == null) continue;

            if (firstParameterAdded)
            {
                queryString.Append('&');
            }
            else
            {
                firstParameterAdded = true;
                queryString.Append('?');
            }

            string propertyName = property.Name.ToEscaped();

            if (loweredPropertyNames)
                propertyName = propertyName.ToLowerInvariantFast();

            queryString.Append(propertyName)
                .Append('=')
                .Append(value.ToString().ToEscaped());
        }

        return queryString.ToString();
    }

    /// <summary>
    /// Builds a query string out of an object by serializing the object and then deserializing into a Dictionary. <para/>
    /// Uses the object's property name OR 'JsonPropertyName' attribute as the keys of the query string. Escapes the value. <para/>
    /// This is recommended over <see cref="ToQueryStringViaReflection"/> as it's slightly faster. <para/>
    /// </summary>
    /// <remarks>This string's first character is a question mark (unless the object is null, then it's null)</remarks>
    /// <returns>If object is null, returns an empty string.</returns>
    [Pure]
    public static string ToQueryString(this object? obj)
    {
        if (obj == null)
            return "";

        string? serializedObj = JsonUtil.Serialize(obj);

        if (string.IsNullOrEmpty(serializedObj))
            return "";

        var dictionary = JsonUtil.Deserialize<Dictionary<string, JsonElement>>(serializedObj!);

        if (dictionary == null || dictionary.Count == 0)
            return "";

        var queryBuilder = new StringBuilder(dictionary.Count * 10);
        queryBuilder.Append('?');

        foreach (KeyValuePair<string, JsonElement> qs in dictionary)
        {
            string value = qs.Value.ValueKind switch
            {
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => qs.Value.ToString().ToEscaped()
            };

            if (queryBuilder.Length > 1)
                queryBuilder.Append('&');

            queryBuilder.Append(qs.Key);
            queryBuilder.Append('=');
            queryBuilder.Append(value);
        }

        return queryBuilder.ToString();
    }
}