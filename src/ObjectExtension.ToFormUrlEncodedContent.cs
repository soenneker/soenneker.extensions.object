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
    private sealed class PropertyMap
    {
        public required string Name { get; init; }
        public required Func<object, object?> Getter { get; init; }
    }

    private static readonly ConcurrentDictionary<System.Type, PropertyMap[]> _cache = new();

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
        PropertyMap[] maps = GetPropertyMaps(type);

        var list = new List<KeyValuePair<string, string>>(maps.Length);

        foreach (PropertyMap map in maps)
        {
            object? value = map.Getter(obj);
            if (value is null)
                continue;

            string stringValue = ConvertToString(value);
            list.Add(new KeyValuePair<string, string>(map.Name, stringValue));
        }

        return new FormUrlEncodedContent(list);
    }

    private static PropertyMap[] GetPropertyMaps(System.Type type)
    {
        return _cache.GetOrAdd(type, static t =>
        {
            PropertyInfo[] props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var maps = new List<PropertyMap>(props.Length);

            foreach (PropertyInfo prop in props)
            {
                if (!prop.CanRead)
                    continue;

                if (prop.GetIndexParameters()
                        .Length != 0)
                    continue; // skip indexers

                // Determine form key name (JsonPropertyName if present)
                string name = prop.Name;

                var jsonNameAttr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();

                if (jsonNameAttr is not null && !string.IsNullOrWhiteSpace(jsonNameAttr.Name))
                    name = jsonNameAttr.Name;

                // Build a fast-ish getter delegate
                MethodInfo getMethod = prop.GetMethod!;
                Func<object, object?> getter;

                if (getMethod.IsStatic)
                {
                    // Unlikely, but handle anyway
                    getter = _ => getMethod.Invoke(null, null);
                }
                else
                {
                    // Create an open delegate where possible
                    System.Type declaringType = getMethod.DeclaringType!;
                    System.Type returnType = getMethod.ReturnType;

                    System.Type delegateType = typeof(Func<,>).MakeGenericType(declaringType, returnType);
                    Delegate del = getMethod.CreateDelegate(delegateType);

                    getter = target => del.DynamicInvoke(target);
                }

                maps.Add(new PropertyMap
                {
                    Name = name,
                    Getter = getter
                });
            }

            return maps.ToArray();
        });
    }

    private static string ConvertToString(object value)
    {
        // Avoid boxing where we can
        return value switch
        {
            string s => s,
            bool b => b ? "true" : "false",
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }
}