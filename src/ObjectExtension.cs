using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Enumerable.String;
using Soenneker.Extensions.String;
using Soenneker.Extensions.Type;
using Soenneker.Utils.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Soenneker.Utils.PooledStringBuilders;

namespace Soenneker.Extensions.Object;

/// <summary>
/// A collection of helpful Object extension methods
/// </summary>
public static partial class ObjectExtension
{
    private static readonly ConcurrentDictionary<System.Type, (PropertyInfo[] Props, string[] Names)> _declaredPropCache = new();
    private static readonly ConcurrentDictionary<System.Type, PropertyInfo[]> _publicPropCache = new();

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

    /// <summary>
    /// Converts an object's public instance properties into a dictionary.
    /// Uses <see cref="JsonPropertyNameAttribute"/> if present for the key.
    /// Only declared properties (not inherited) are included.
    /// Optimized for low allocations and high performance.
    /// </summary>
    /// <param name="source">The object to convert.</param>
    /// <returns>A dictionary of property names (or JSON names) to values.</returns>
    [Pure]
    public static Dictionary<string, object?> ToDictionary(this object? source)
    {
        if (source is null)
            return new Dictionary<string, object?>();

        System.Type type = source.GetType();

        // Cache DECLARED ONLY + json names
        (PropertyInfo[] props, string[] names) = _declaredPropCache.GetOrAdd(type, t =>
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
            PropertyInfo[] raw = t.GetProperties(flags);
            // Filter to readable, non-indexer once
            PropertyInfo[] filtered = raw.Where(p => p.CanRead && p.GetIndexParameters()
                    .Length == 0)
                .ToArray();
            var nameArr = new string[filtered.Length];
            for (var i = 0; i < filtered.Length; i++)
                nameArr[i] = filtered[i]
                    .GetCustomAttribute<JsonPropertyNameAttribute>(false)
                    ?.Name ?? filtered[i].Name;
            return (filtered, nameArr);
        });

        var dict = new Dictionary<string, object?>(props.Length);

        for (var i = 0; i < props.Length; i++)
        {
            dict[names[i]] = props[i]
                .GetValue(source);
        }

        return dict;
    }

    /// <summary>
    /// Uses Reflection to build a query string out of an object. If object is null, returns an empty string. Uses the object's property names as the keys of the query string.
    /// </summary>
    /// <remarks>This string's first character is a question mark (unless the object is null, then it's null)</remarks>
    [Pure]
    public static string ToQueryStringViaReflection(this object? obj, bool loweredPropertyNames = true)
    {
        if (obj is null)
            return string.Empty;

        var type = obj.GetType();
        var props = _publicPropCache.GetOrAdd(type, t =>
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            return t.GetProperties(flags)
                .Where(p => p.CanRead && p.GetIndexParameters()
                    .Length == 0)
                .ToArray();
        });

        using var sb = new PooledStringBuilder();

        var any = false;

        for (var i = 0; i < props.Length; i++)
        {
            var p = props[i];
            var val = p.GetValue(obj);
            if (val is null)
                continue;

            sb.Append(any ? '&' : '?');
            any = true;

            var name = p.Name.ToEscaped();
            if (loweredPropertyNames)
                name = name.ToLowerInvariantFast(); // already your fast path

            sb.Append(name);
            sb.Append('=');
            sb.Append(val.ToString()
                .ToEscaped());
        }

        return any ? sb.ToString() : string.Empty;
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
        if (obj is null)
            return "";

        string? serializedObj = JsonUtil.Serialize(obj);

        if (serializedObj.IsNullOrEmpty())
            return "";

        var dictionary = JsonUtil.Deserialize<Dictionary<string, JsonElement>>(serializedObj);

        if (dictionary is null || dictionary.Count == 0)
            return "";

        var queryBuilder = new StringBuilder(dictionary.Count * 10);
        queryBuilder.Append('?');

        foreach (KeyValuePair<string, JsonElement> qs in dictionary)
        {
            string value = qs.Value.ValueKind switch
            {
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => qs.Value.ToString()
                    .ToEscaped()
            };

            if (queryBuilder.Length > 1)
                queryBuilder.Append('&');

            queryBuilder.Append(qs.Key)
                .Append('=')
                .Append(value);
        }

        return queryBuilder.ToString();
    }

    /// <summary>
    /// Logs any properties of the given object that are null.
    /// </summary>
    /// <param name="obj">The object to inspect for null properties. Can be null.</param>
    /// <param name="logger">The logger to use for logging null properties.</param>
    public static void LogNullProperties(this object? obj, ILogger logger)
    {
        if (obj is null)
        {
            logger.LogWarning("LogNullProperties: Object is null");
            return;
        }

        System.Type objectType = obj.GetType();
        var nullProperties = new List<string>();

        foreach (PropertyInfo property in objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetValue(obj) is null)
            {
                nullProperties.Add($"{property.Name} (Type: {property.PropertyType.Name})");
            }
        }

        if (nullProperties.Count > 0)
        {
            string nullPropertiesString = nullProperties.ToCommaSeparatedString(true);
            logger.LogInformation("LogNullProperties: Type ({parentType}), Null properties: {nullPropertiesString}", objectType.FullName, nullPropertiesString);
        }
        else
        {
            logger.LogInformation("LogNullProperties: Type: {parentType}, No null properties found", objectType.FullName);
        }
    }

    /// <summary>
    /// Logs the properties of an object that are null, including nested objects, as a JSON string.
    /// </summary>
    /// <param name="obj">The object to inspect for null properties.</param>
    /// <param name="logger">The logger to use for logging the null properties.</param>
    public static void LogNullPropertiesRecursivelyAsJson(this object? obj, ILogger logger)
    {
        if (obj is null)
        {
            logger.LogWarning("LogNullPropertiesAsJson: Object is null");
            return;
        }

        System.Type objectType = obj.GetType();

        Dictionary<string, object?> nullPropertiesTree = GetNullPropertiesTree(obj, objectType, new HashSet<object>());

        if (nullPropertiesTree.Count > 0)
        {
            string? jsonString = JsonUtil.Serialize(nullPropertiesTree);
            logger.LogInformation("LogNullPropertiesAsJson: Type ({objectType}), Null properties tree: {jsonString}", objectType.FullName, jsonString);
        }
        else
        {
            logger.LogInformation("LogNullPropertiesAsJson: Type: {objectType}, No null properties found", objectType.FullName);
        }
    }

    private static Dictionary<string, object?> GetNullPropertiesTree(object obj, System.Type objectType, HashSet<object> visitedObjects)
    {
        var nullPropertiesTree = new Dictionary<string, object?>();

        if (!visitedObjects.Add(obj))
        {
            return nullPropertiesTree; // Prevent infinite recursion
        }

        System.Type stringType = typeof(string);

        foreach (PropertyInfo property in objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Skip properties that have index parameters
            if (property.GetIndexParameters()
                    .Length > 0)
            {
                continue;
            }

            object? value = property.GetValue(obj);

            if (value is null)
            {
                nullPropertiesTree.Add(property.Name, null);
            }
            else if (!property.PropertyType.IsValueType && property.PropertyType != stringType)
            {
                Dictionary<string, object?> subNullPropertiesTree = GetNullPropertiesTree(value, property.PropertyType, visitedObjects);
                if (subNullPropertiesTree.Count > 0)
                {
                    nullPropertiesTree.Add(property.Name, subNullPropertiesTree);
                }
            }
            else if (value is IEnumerable enumerable and not string)
            {
                var subNullPropertiesList = new List<object?>();
                foreach (object? item in enumerable)
                {
                    if (item is not null)
                    {
                        Dictionary<string, object?> itemNullPropertiesTree = GetNullPropertiesTree(item, item.GetType(), visitedObjects);

                        if (itemNullPropertiesTree.Count > 0)
                        {
                            subNullPropertiesList.Add(itemNullPropertiesTree);
                        }
                    }
                    else
                    {
                        subNullPropertiesList.Add(null);
                    }
                }

                if (subNullPropertiesList.Count > 0)
                {
                    nullPropertiesTree.Add(property.Name, subNullPropertiesList);
                }
            }
        }

        return nullPropertiesTree;
    }

    /// <summary>
    /// Converts an object into a human-readable string representation, including its public properties.
    /// </summary>
    /// <param name="obj">The object to be converted into a readable string.</param>
    /// <param name="indentLevel">The indentation level used for formatting nested objects and collections. Default is 0.</param>
    /// <returns>A formatted string representation of the object's properties and their values.</returns>
    /// <remarks>
    /// - If the object is <c>null</c>, the method returns the string <c>"null"</c>.
    /// - If the object contains enumerable properties (excluding strings), each element is indented and listed.
    /// - If the object contains nested class instances, they are recursively processed with increased indentation.
    /// - Primitive and string properties are displayed directly alongside their values.
    /// </remarks>
    /// <example>
    /// Example usage:
    /// <code>
    /// var person = new Person { Name = "Alice", Age = 30 };
    /// string readableString = person.ToReadableString();
    /// Console.WriteLine(readableString);
    /// </code>
    /// Output:
    /// <code>
    /// Name: Alice
    /// Age: 30
    /// </code>
    /// </example>
    [Pure]
    public static string ToReadableString(this object obj, int indentLevel = 0)
    {
        if (obj == null)
            return "null";

        System.Type type = obj.GetType();
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var stringBuilder = new StringBuilder();

        var indent = new string(' ', indentLevel * 2);

        foreach (PropertyInfo property in properties)
        {
            if (property.GetIndexParameters()
                    .Length == 0)
            {
                object? value = property.GetValue(obj, null);
                var propertyName = $"{indent}{property.Name}:";

                if (value == null)
                {
                    stringBuilder.AppendLine($"{propertyName} null");
                }
                else if (value is IEnumerable enumerable and not string)
                {
                    stringBuilder.AppendLine(propertyName);

                    foreach (object? item in enumerable)
                    {
                        stringBuilder.Append(item.ToReadableString(indentLevel + 1));
                    }
                }
                else if (value.GetType()
                             .IsClass && !value.GetType()
                             .IsPrimitive && value is not string)
                {
                    stringBuilder.AppendLine(propertyName);
                    stringBuilder.Append(value.ToReadableString(indentLevel + 1));
                }
                else
                {
                    stringBuilder.AppendLine($"{propertyName} {value}");
                }
            }
        }

        return stringBuilder.ToString();
    }
}