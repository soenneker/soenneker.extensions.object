using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Soenneker.Benchmarking.Extensions.Summary;
using Soenneker.Tests.Benchmark;

using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Soenneker.Benchmarking.Extensions.Summary;
using Soenneker.Tests.Benchmark;

namespace Soenneker.Extensions.Object.Tests.Benchmarks;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class BenchmarkTestRunner : BenchmarkTest
{
    public BenchmarkTestRunner(Host host) : base()
    {
    }

    [Skip("Manual")]
    //[LocalOnly]
    public async System.Threading.Tasks.Task ExtensionBenchmarks()
    {
        Summary summary = BenchmarkRunner.Run<ExtensionBenchmarks>(DefaultConf);

        await summary.OutputSummaryToLog();
    }
}



