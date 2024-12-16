using FluentAssertions;
using Soenneker.Extensions.Object.Tests.Dtos;
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

    [Fact]
    public void ToQueryString_lowercase_bool()
    {
        var user = AutoFaker.Generate<UserDto>();
        user.IsActive = true;

        string result = user.ToQueryString();
        result.Should().NotContain("True");
    }

    [Fact]
    public void LogNullProperties_should_log()
    {
        UserDto obj = AutoFaker.Generate<UserDto>();
        obj.Address.AdditionalInfo = null;

        obj.LogNullProperties(Logger);
    }

    [Fact]
    public void LogNullPropertiesRecursivelyAsJson_should_log()
    {
        UserDto obj = AutoFaker.Generate<UserDto>();
        obj.Address.AdditionalInfo = null!;
        obj.PhoneNumber = null!;

        obj.LogNullPropertiesRecursivelyAsJson(Logger);
    }
}