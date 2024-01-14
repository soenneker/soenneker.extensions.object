using BenchmarkDotNet.Attributes;
using Soenneker.Utils.AutoBogus;

namespace Soenneker.Extensions.Object.Tests.Benchmarks;

public class ExtensionBenchmarks
{
    [Benchmark]
    public void ToQueryString()
    {
        var user = AutoFaker.Generate<UserDto>();

        _ = user.ToQueryString();
    }

    [Benchmark]
    public void ToQueryStringViaReflection()
    {
        var user = AutoFaker.Generate<UserDto>();

        _ = user.ToQueryStringViaReflection();
    }
}