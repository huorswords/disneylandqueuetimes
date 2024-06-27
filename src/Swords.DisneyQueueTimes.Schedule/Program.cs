using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Swords.DisneyQueueTimes.Schedule.Configuration;
using Swords.DisneyQueueTimes.Schedule.Parks;

namespace Swords.DisneyQueueTimes.Schedule;

public class Program
{
    public static void Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging();
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();

                var configuration = context.Configuration;

                services.Configure<SynchronizationOptions>(configuration.GetSection(nameof(SynchronizationOptions)));
                services.Configure<QueueTimesApiOptions>(configuration.GetSection(nameof(QueueTimesApiOptions)));
                services.AddLogging();
                services.AddSingleton<SynchronizationLocker>();
                services.AddHttpClient<QueueTimesClient>((provider, httpClient) =>
                {
                    var apiOptions = provider.GetRequiredService<IOptions<QueueTimesApiOptions>>();
                    httpClient.BaseAddress = new Uri(apiOptions.Value.BaseAddress);
                });
            })
            .ConfigureAppConfiguration(builder => builder.AddEnvironmentVariables()
                                                         .AddUserSecrets(typeof(Program).Assembly))
            .Build();

        host.Run();
    }
}