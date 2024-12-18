using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using Microsoft.Extensions.Logging;
using Soenneker.Tests.FixturedUnit;
using Soenneker.Utils.File.Abstract;
using Xunit;


namespace Soenneker.Extensions.Object.Tests.Benchmarks;

[Collection("Collection")]
public class BenchmarkTest : FixturedUnitTest
{
    protected ManualConfig DefaultConf { get; }

    protected BenchmarkTest(Fixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
        DefaultConf = ManualConfig.Create(DefaultConfig.Instance).WithOptions(ConfigOptions.DisableOptimizationsValidator);
    }

    protected async System.Threading.Tasks.ValueTask OutputSummaryToLog(Summary summary)
    {
        var fileUtil = Resolve<IFileUtil>();

        string log = await fileUtil.ReadFile(summary.LogFilePath);

        Logger.LogInformation("{log}", log);
    }
}