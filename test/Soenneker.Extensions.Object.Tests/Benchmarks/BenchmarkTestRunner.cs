using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Soenneker.Benchmarking.Extensions.Summary;
using Soenneker.Facts.Manual;
using Soenneker.Tests.Benchmark;
using Xunit;


namespace Soenneker.Extensions.Object.Tests.Benchmarks;

[Collection("Collection")]
public class BenchmarkTestRunner : BenchmarkTest
{
    public BenchmarkTestRunner(Fixture fixture, ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [ManualFact]
    //[LocalFact]
    public async System.Threading.Tasks.Task ExtensionBenchmarks()
    {
        Summary summary = BenchmarkRunner.Run<ExtensionBenchmarks>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }
}