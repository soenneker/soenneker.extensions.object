using AwesomeAssertions;
using Soenneker.Extensions.Object.Tests.Dtos;
using Soenneker.Tests.HostedUnit;
using Soenneker.Utils.Json;
using System;
using System.Text.Json.Serialization;
using System.Threading;

namespace Soenneker.Extensions.Object.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class ObjectExtensionTests : HostedUnitTest
{
    public ObjectExtensionTests(Host host) : base(host)
    {
    }

    [Test]
    public void Default(){}

    [Test]
    public void ToHttpContent_should_not_throw()
    {
        var obj = AutoFaker.Generate<UserDto>();

        var result = obj.ToHttpContent(TestJsonContext.Get<UserDto>());
        result.Should().NotBeNull();
    }

    [Test]
    public async System.Threading.Tasks.ValueTask ToHttpContent_should_deserialize(CancellationToken cancellationToken)
    {
        var obj = AutoFaker.Generate<UserDto>();

        var result = obj.ToHttpContent(TestJsonContext.Get<UserDto>());
        string content = await result.ReadAsStringAsync(cancellationToken);
        JsonUtil.Deserialize<UserDto>(content).Should().BeEquivalentTo(obj);
    }

}
