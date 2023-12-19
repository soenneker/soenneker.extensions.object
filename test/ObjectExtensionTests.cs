using FluentAssertions;
using Soenneker.Extensions.Object.Tests.Benchmarks;
using Soenneker.Tests.FixturedUnit;
using Xunit;
using Xunit.Abstractions;

namespace Soenneker.Extensions.Object.Tests;

[Collection("Collection")]
public class ObjectExtensionTests : FixturedUnitTest
{
    public ObjectExtensionTests(Fixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public void ToQueryStringViaReflection_handles_null()
    {
        var user = AutoFaker.Generate<UserDto>();
        user.FirstName = null;

        string result = user.ToQueryStringViaReflection();
        result.Should().NotContain("firstName");
    }

    [Fact]
    public void ToQueryString_handles_null()
    {
        var user = AutoFaker.Generate<UserDto>();
        user.FirstName = null;

        string result = user.ToQueryString();
        result.Should().NotContain("firstName");
    }
}