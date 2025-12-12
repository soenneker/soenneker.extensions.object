using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Soenneker.Extensions.Object;

public static partial class ObjectExtension
{
    private static readonly ConcurrentDictionary<System.Type, (PropertyInfo[] Props, string[] Names)> _formUrlEncodedPropCache = new();

    /// <summary>
    /// Converts an object into FormUrlEncodedContent using reflection,
    /// honoring JsonPropertyName attributes on properties.
    /// Only top-level, readable instance properties are included.
    /// </summary>
    public static FormUrlEncodedContent ToFormUrlEncodedContent(this object obj)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));

        System.Type type = obj.GetType();

        // Cache properties and names
        (PropertyInfo[] props, string[] names) = _formUrlEncodedPropCache.GetOrAdd(type, static t =>
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            PropertyInfo[] raw = t.GetProperties(flags);
            
            // Filter to readable, non-indexer properties - avoid intermediate list allocation
            var filtered = new List<PropertyInfo>(raw.Length);
            for (var i = 0; i < raw.Length; i++)
            {
                PropertyInfo prop = raw[i];
                if (prop.CanRead && prop.GetIndexParameters().Length == 0)
                    filtered.Add(prop);
            }

            int count = filtered.Count;
            PropertyInfo[] propsArray = new PropertyInfo[count];
            var nameArr = new string[count];
            for (var i = 0; i < count; i++)
            {
                PropertyInfo prop = filtered[i];
                propsArray[i] = prop;
                nameArr[i] = prop.GetCustomAttribute<JsonPropertyNameAttribute>(false)?.Name ?? prop.Name;
            }

            return (propsArray, nameArr);
        });

        var list = new List<KeyValuePair<string, string>>(props.Length);

        for (var i = 0; i < props.Length; i++)
        {
            object? value = props[i].GetValue(obj);
            if (value is null)
                continue;

            string stringValue = ConvertToString(value);
            list.Add(new KeyValuePair<string, string>(names[i], stringValue));
        }

        return new FormUrlEncodedContent(list);
    }

    private static string ConvertToString(object value)
    {
        return value switch
        {
            string s => s,
            bool b => b ? "true" : "false",
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }
}