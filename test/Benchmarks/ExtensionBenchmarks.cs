using BenchmarkDotNet.Attributes;
using Soenneker.Utils.AutoBogus;

namespace Soenneker.Extensions.Object.Tests.Benchmarks;

public class ExtensionBenchmarks
{
    private AutoFaker _autoFaker;

    [GlobalSetup]
    public void SetupData()
    {
        _autoFaker = new AutoFaker();
    }

    [Benchmark]
    public void ToQueryString()
    {
        var user = _autoFaker.Generate<UserDto>();

        _ = user.ToQueryString();
    }

    [Benchmark]
    public void ToQueryStringViaReflection()
    {
        var user = _autoFaker.Generate<UserDto>();

        _ = user.ToQueryStringViaReflection();
    }
}