using AwesomeAssertions;
using Soenneker.Extensions.Object.Tests.Dtos;
using Soenneker.Tests.FixturedUnit;
using Soenneker.Utils.Json;
using Xunit;


namespace Soenneker.Extensions.Object.Tests;

[Collection("Collection")]
public class ObjectExtensionTests : FixturedUnitTest
{
    public ObjectExtensionTests(Fixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public void Default()
    {

    }

    [Fact]
    public void ToHttpContent_should_not_throw()
    {
        var obj = AutoFaker.Generate<UserDto>();

        var result = obj.ToHttpContent();
        result.Should().NotBeNull();
    }
}