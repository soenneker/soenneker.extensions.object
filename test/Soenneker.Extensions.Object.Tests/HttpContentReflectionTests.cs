using AwesomeAssertions;
using System.Net.Http;
using System.Text.Json;


namespace Soenneker.Extensions.Object.Tests;

public class HttpContentReflectionTests
{
    [Test]
    public async System.Threading.Tasks.Task ToHttpContent_serializes_anonymous_objects()
    {
        var payload = new { Query = "mutation", Variables = new { RepositoryId = "node\"id", Enabled = false } };
        using HttpContent content = payload.ToHttpContent();
        using JsonDocument document = JsonDocument.Parse(await content.ReadAsStringAsync());

        content.Headers.ContentType!.MediaType.Should().Be("application/json");
        document.RootElement.GetProperty("query").GetString().Should().Be("mutation");
        JsonElement variables = document.RootElement.GetProperty("variables");
        variables.GetProperty("repositoryId").GetString().Should().Be("node\"id");
        variables.GetProperty("enabled").GetBoolean().Should().BeFalse();
    }

    [Test]
    public async System.Threading.Tasks.Task ToHttpContent_returns_empty_json_content_for_null()
    {
        using HttpContent content = ((object?)null).ToHttpContent();

        (await content.ReadAsByteArrayAsync()).Should().BeEmpty();
        content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }
}
