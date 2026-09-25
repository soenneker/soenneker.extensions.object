using AwesomeAssertions;
using Soenneker.Extensions.Object.Tests.Dtos;
using Soenneker.Tests.HostedUnit;
using Soenneker.Utils.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using System.Text.Json;
using System;

namespace Soenneker.Extensions.Object.Tests;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, ReadCommentHandling = JsonCommentHandling.Skip, UseStringEnumConverter = true)]
[JsonSerializable(typeof(UserDto))]
internal partial class TestJsonContext : JsonSerializerContext
{
    internal static JsonTypeInfo<T> Get<T>() =>
        (JsonTypeInfo<T>)(Default.GetTypeInfo(typeof(T)) ?? throw new NotSupportedException($"No generated JSON metadata for {typeof(T)}."));
}
