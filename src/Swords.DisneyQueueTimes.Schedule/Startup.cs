using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swords.DisneyQueueTimes.Schedule;
using Swords.DisneyQueueTimes.Schedule.Configuration;
using Swords.DisneyQueueTimes.Schedule.Parks;
using System;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Swords.DisneyQueueTimes.Schedule;

public sealed class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.GetContext().Configuration;

        services.Configure<SynchronizationOptions>(configuration.GetSection(nameof(SynchronizationOptions)));
        services.Configure<QueueTimesApiOptions>(configuration.GetSection(nameof(QueueTimesApiOptions)));
        services.AddLogging();
        services.AddSingleton<SynchronizationLocker>();
        services.AddHttpClient<QueueTimesClient>((provider, httpClient) =>
        {
            var apiOptions = provider.GetService<IOptions<QueueTimesApiOptions>>();
            httpClient.BaseAddress = new Uri(apiOptions.Value.BaseAddress);
        });
    }
}
