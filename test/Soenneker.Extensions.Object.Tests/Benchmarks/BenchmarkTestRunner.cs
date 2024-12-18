using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Soenneker.Facts.Local;
using Xunit;


namespace Soenneker.Extensions.Object.Tests.Benchmarks;

[Collection("Collection")]
public class BenchmarkTestRunner : BenchmarkTest
{
    public BenchmarkTestRunner(Fixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    //[ManualFact]
    [LocalFact]
    public async System.Threading.Tasks.Task ExtensionBenchmarks()
    {
        Summary summary = BenchmarkRunner.Run<ExtensionBenchmarks>(DefaultConf);

        await OutputSummaryToLog(summary);
    }
}