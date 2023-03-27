using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using Soenneker.Extensions.Type;
using Soenneker.Utils.Json;
using Soenneker.Utils.Json.Abstract;

namespace Soenneker.Extensions.Object;

public static class ObjectExtension
{
    /// <summary>
    /// Returns a application/json HttpContent, with JsonUtil.WebOptions. If the object is null, returns a new HttpContent with empty content
    /// </summary>
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
}