using System;
using System.Globalization;
using System.Text.Json.Serialization;
using AwesomeAssertions;

namespace Soenneker.Extensions.Object.Tests;

public class QueryStringTests
{
    [Test]
    public void ToQueryString_honors_metadata_and_escapes_names_and_values()
    {
        var model = new QueryModel { Name = "a & b+", Enabled = true, Amount = 12.5m, Values = [1, 2] };
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            model.ToQueryString(QueryStringJsonContext.Default.QueryModel).Should()
                .Be("?name%26key=a%20%26%20b%2B&enabled=true&amount=12.5&values=%5B1%2C2%5D&explicitNull=");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Test]
    public void ToQueryString_returns_empty_for_null_and_empty_objects()
    {
        QueryModel? model = null;
        model.ToQueryString(QueryStringJsonContext.Default.QueryModel).Should().BeEmpty();
        new EmptyQueryModel().ToQueryString(QueryStringJsonContext.Default.EmptyQueryModel).Should().BeEmpty();
    }

    [Test]
    public void ToQueryString_rejects_non_object_roots()
    {
        Action action = () => 42.ToQueryString(QueryStringJsonContext.Default.Int32);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void ToQueryString_requires_metadata()
    {
        Action action = () => new QueryModel().ToQueryString(null!);
        action.Should().Throw<ArgumentNullException>();
    }
}

public sealed class QueryModel
{
    [JsonPropertyName("name&key")]
    public string? Name { get; init; }
    public bool Enabled { get; init; }
    public decimal Amount { get; init; }
    public int[]? Values { get; init; }
    public string? OmittedNull { get; init; }
    [JsonIgnore]
    public string Ignored => "ignored";
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int OmittedDefault { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? ExplicitNull { get; init; }
}

public sealed class EmptyQueryModel;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(QueryModel))]
[JsonSerializable(typeof(EmptyQueryModel))]
[JsonSerializable(typeof(int))]
internal partial class QueryStringJsonContext : JsonSerializerContext;
