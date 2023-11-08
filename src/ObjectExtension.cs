using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection;
using System.Text;
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
        StringContent result;

        if (obj == null)
        {
            result = new StringContent("", Encoding.UTF8, MediaTypeNames.Application.Json);
            return result;
        }

        string? stringContent = JsonUtil.Serialize(obj);
        result = new StringContent(stringContent!, Encoding.UTF8, MediaTypeNames.Application.Json);
        return result;
    }

    /// <returns>An application/json HttpContent, with JsonUtil.WebOptions. <para/>
    /// If the object is null, returns a new HttpContent with empty content. <para/>
    /// Also returns the serialized string that was used to construct the HttpContent.
    /// </returns>
    [Pure]
    public static (HttpContent httpContent, string str) ToHttpContentAndString(this object? obj)
    {
        StringContent result;

        if (obj == null)
        {
            const string resultStr = "";
            result = new StringContent(resultStr, Encoding.UTF8, MediaTypeNames.Application.Json);
            return (result, resultStr);
        }

        string stringContent = JsonUtil.Serialize(obj)!;
        result = new StringContent(stringContent, Encoding.UTF8, MediaTypeNames.Application.Json);
        return (result, stringContent);
    }

    [Pure]
    public static bool IsObjectNumeric(this object obj)
    {
        return obj.GetType().IsNumeric();
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
    /// <see cref="ToHttpContent"/> and then adds the 'x-api-key' header to the request
    /// </summary>
    [Pure]
    public static HttpContent ToHttpContentWithKey(this object? obj, string apiKey)
    {
        var httpContent = obj.ToHttpContent();
        httpContent.Headers.Add(AuthConstants.XApiKey, apiKey);

        return httpContent;
    }

    /// <summary>
    /// Builds a query string out of an object. If object is null, returns an empty string.
    /// </summary>
    /// <remarks>This string's first character is a question mark (unless the object is null, then it's null)</remarks>
    [Pure]
    public static string ToQueryString(this object? obj, bool loweredPropertyNames = true)
    {
        if (obj == null)
            return "";

        var queryString = new StringBuilder();
        System.Type type = obj.GetType();
        PropertyInfo[] properties = type.GetProperties();

        foreach (PropertyInfo property in properties)
        {
            object? value = property.GetValue(obj);

            if (value == null) 
                continue;
            
            if (queryString.Length > 0)
                queryString.Append('&');

            string propertyName = property.Name.ToEscaped()!;

            if (loweredPropertyNames)
                propertyName = propertyName.ToLowerInvariant();

            queryString.Append(propertyName);
            queryString.Append('=');
            queryString.Append(value.ToString().ToEscaped());
        }

        return '?' + queryString.ToString();
    }
}