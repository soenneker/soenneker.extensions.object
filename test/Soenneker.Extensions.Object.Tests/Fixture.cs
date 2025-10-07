using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.XUnit.Injectable.Abstract;
using Soenneker.Fixtures.Unit;
using Soenneker.Utils.Test;

namespace Soenneker.Extensions.Object.Tests;

public class Fixture : UnitFixture
{
    public override async System.Threading.Tasks.ValueTask InitializeAsync()
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
        if (ServiceProvider != null)
        {
            var sink = ServiceProvider.GetService<IInjectableTestOutputSink>();
            if (sink != null)
                await sink.DisposeAsync();
        }

        await base.DisposeAsync();
    }
}