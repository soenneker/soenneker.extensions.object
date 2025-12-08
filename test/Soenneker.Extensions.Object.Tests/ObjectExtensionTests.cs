using AwesomeAssertions;
using Soenneker.Extensions.Object.Tests.Dtos;
using Soenneker.Tests.FixturedUnit;
using Soenneker.Utils.Json;
using System;
using Xunit;


namespace Soenneker.Extensions.Object.Tests;

[Collection("Collection")]
public class ObjectExtensionTests : FixturedUnitTest
{
    public ObjectExtensionTests(Fixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public void Default(){}

    [Fact]
    public void ToHttpContent_should_not_throw()
    {
        var obj = AutoFaker.Generate<UserDto>();

        var result = obj.ToHttpContent();
        result.Should().NotBeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task ToHttpContent_should_deserialize()
    {
        var obj = AutoFaker.Generate<UserDto>();

        var result = obj.ToHttpContent();
        string content = await result.ReadAsStringAsync(CancellationToken);
        JsonUtil.Deserialize<UserDto>(content).Should().BeEquivalentTo(obj);
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
        var obj = AutoFaker.Generate<UserDto>();
        obj.Address.AdditionalInfo = null;

        obj.LogNullProperties(Logger);
    }

    [Fact]
    public void LogNullPropertiesRecursivelyAsJson_should_log()
    {
        var obj = AutoFaker.Generate<UserDto>();
        obj.Address.AdditionalInfo = null!;
        obj.PhoneNumber = null!;

        obj.LogNullPropertiesRecursivelyAsJson(Logger);
    }

    [Fact]
    public void ToFormUrlEncodedContent_should_throw_on_null()
    {
        object? obj = null;

        Action act = () => obj!.ToFormUrlEncodedContent();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async System.Threading.Tasks.Task ToFormUrlEncodedContent_should_create_content()
    {
        var obj = new { Name = "Test", Value = 123 };

        var result = obj.ToFormUrlEncodedContent();
        result.Should().NotBeNull();

        string content = await result.ReadAsStringAsync(CancellationToken);
        content.Should().Contain("Name=Test");
        content.Should().Contain("Value=123");
    }

    [Fact]
    public async System.Threading.Tasks.Task ToFormUrlEncodedContent_should_use_json_property_name()
    {
        var user = AutoFaker.Generate<UserDto>();

        var result = user.ToFormUrlEncodedContent();
        string content = await result.ReadAsStringAsync(CancellationToken);

        // Should use "firstName" from JsonPropertyName attribute, not "FirstName"
        if (user.FirstName != null)
        {
            content.Should().Contain("firstName=");
            content.Should().NotContain("FirstName=");
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task ToFormUrlEncodedContent_should_skip_null_properties()
    {
        var user = AutoFaker.Generate<UserDto>();
        user.FirstName = null;

        var result = user.ToFormUrlEncodedContent();
        string content = await result.ReadAsStringAsync(CancellationToken);

        content.Should().NotContain("firstName");
    }

    [Fact]
    public async System.Threading.Tasks.Task ToFormUrlEncodedContent_should_handle_bool_values()
    {
        var obj = new { IsActive = true, IsDeleted = false };

        var result = obj.ToFormUrlEncodedContent();
        string content = await result.ReadAsStringAsync(CancellationToken);

        content.Should().Contain("IsActive=true");
        content.Should().Contain("IsDeleted=false");
    }

    [Fact]
    public async System.Threading.Tasks.Task ToFormUrlEncodedContent_should_handle_numeric_types()
    {
        var obj = new
        {
            IntValue = 42,
            DecimalValue = 123.45m,
            DoubleValue = 67.89,
            FloatValue = 10.5f
        };

        var result = obj.ToFormUrlEncodedContent();
        string content = await result.ReadAsStringAsync(CancellationToken);

        content.Should().Contain("IntValue=42");
        content.Should().Contain("DecimalValue=123.45");
        content.Should().Contain("DoubleValue=67.89");
        content.Should().Contain("FloatValue=10.5");
    }

    [Fact]
    public async System.Threading.Tasks.Task ToFormUrlEncodedContent_should_handle_datetime()
    {
        var dateTime = new DateTime(2024, 1, 15, 10, 30, 0);
        var obj = new { CreatedAt = dateTime };

        var result = obj.ToFormUrlEncodedContent();
        string content = await result.ReadAsStringAsync(CancellationToken);

        // DateTime should be formatted using InvariantCulture
        content.Should().Contain("CreatedAt=");
        content.Should().Contain("2024");
    }

    [Fact]
    public async System.Threading.Tasks.Task ToFormUrlEncodedContent_should_handle_empty_object()
    {
        var obj = new { };

        var result = obj.ToFormUrlEncodedContent();
        string content = await result.ReadAsStringAsync(CancellationToken);

        content.Should().BeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task ToFormUrlEncodedContent_should_handle_object_with_all_null_properties()
    {
        var obj = new { Name = (string?)null, Value = (int?)null };

        var result = obj.ToFormUrlEncodedContent();
        string content = await result.ReadAsStringAsync(CancellationToken);

        content.Should().BeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task ToFormUrlEncodedContent_should_handle_string_with_special_characters()
    {
        var obj = new { Message = "Hello & World" };

        var result = obj.ToFormUrlEncodedContent();
        string content = await result.ReadAsStringAsync(CancellationToken);

        // FormUrlEncodedContent should URL-encode special characters
        content.Should().Contain("Message=");
    }

    [Fact]
    public async System.Threading.Tasks.Task ToFormUrlEncodedContent_should_handle_multiple_properties()
    {
        var user = AutoFaker.Generate<UserDto>();
        user.FirstName = "John";
        user.LastName = "Doe";
        user.IsActive = true;
        user.UserId = 123;

        var result = user.ToFormUrlEncodedContent();
        string content = await result.ReadAsStringAsync(CancellationToken);

        content.Should().Contain("firstName=John");
        content.Should().Contain("LastName=Doe");
        content.Should().Contain("IsActive=true");
        content.Should().Contain("UserId=123");
    }

    [Fact]
    public async System.Threading.Tasks.Task ToFormUrlEncodedContent_should_have_correct_content_type()
    {
        var obj = new { Name = "Test" };

        var result = obj.ToFormUrlEncodedContent();
        result.Headers.ContentType!.MediaType.Should().Be("application/x-www-form-urlencoded");
    }
}