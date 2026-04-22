using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Soenneker.TestHosts.Unit;
using Soenneker.Utils.Test;

namespace Soenneker.Extensions.Object.Tests;

public class Host : UnitTestHost
{
    public override async System.Threading.Tasks.Task InitializeAsync()
    {
        SetupIoC(Services);

        await base.InitializeAsync();
    }

    private static void SetupIoC(IServiceCollection services)
    {
        services.AddLogging(builder => { builder.AddSerilog(dispose: false); });

        IConfiguration config = TestUtil.BuildConfig();
        services.AddSingleton(config);
    }

    public override async System.Threading.Tasks.ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
    }
}
