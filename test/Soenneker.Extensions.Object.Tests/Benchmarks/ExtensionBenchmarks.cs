using BenchmarkDotNet.Attributes;
using Soenneker.Extensions.Object.Tests.Dtos;
using Soenneker.Utils.AutoBogus;

namespace Soenneker.Extensions.Object.Tests.Benchmarks;

public class ExtensionBenchmarks
{
    private AutoFaker _autoFaker = null!;

    private UserDto _userDto = null!;

    [GlobalSetup]
    public void SetupData()
    {
        _autoFaker = new AutoFaker();
        _userDto = _autoFaker.Generate<UserDto>();
    }

    [Benchmark]
    public string ToQueryString()
    {
        return _userDto.ToQueryString();
    }

    [Benchmark]
    public string ToQueryStringViaReflection()
    {
        return _userDto.ToQueryStringViaReflection();
    }
}