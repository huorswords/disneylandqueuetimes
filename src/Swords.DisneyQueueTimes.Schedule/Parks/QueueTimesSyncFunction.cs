using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Options;
using Swords.DisneyQueueTimes.Schedule.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Swords.DisneyQueueTimes.Schedule.Parks;

public class QueueTimesSyncFunction
{
    private readonly QueueTimesClient _queueTimesClient;
    private readonly SynchronizationLocker _synchronizationLocker;
    private readonly SynchronizationOptions _options;

    public QueueTimesSyncFunction(QueueTimesClient queueTimesClient,
                                  SynchronizationLocker synchronizationLocker,
                                  IOptions<SynchronizationOptions> options)
    {
        _queueTimesClient = queueTimesClient ?? throw new ArgumentNullException(nameof(queueTimesClient));
        _synchronizationLocker = synchronizationLocker ?? throw new ArgumentNullException(nameof(synchronizationLocker));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    [FunctionName("Sync")]
    public async Task Sync([TimerTrigger("0 */10 * * * *")] TimerInfo myTimer,
                           CancellationToken cancellationToken)
    {
        var disneyLandWaitTimes = _queueTimesClient.GetQueueTimesFor(KnownParks.DisneyLandParis, cancellationToken);
        var disneyStudiosWaitTimes = _queueTimesClient.GetQueueTimesFor(KnownParks.DisneyStudiosParis, cancellationToken);

        await Task.WhenAll(disneyLandWaitTimes, disneyStudiosWaitTimes);

        await LockAndSaveResults(disneyLandWaitTimes.Result, $"DL.{DateTime.UtcNow.Ticks}.json", SynchronizationOptions.WaitTimesBlobContainer, cancellationToken);
        await LockAndSaveResults(disneyStudiosWaitTimes.Result, $"DS.{DateTime.UtcNow.Ticks}.json", SynchronizationOptions.WaitTimesBlobContainer, cancellationToken);
    }

    [FunctionName("UpdateSeries")]
    public async Task UpdateSeries(
        [BlobTrigger("wait-times/{name}",
                     Connection = "SynchronizationOptions:ConnectionString",
                     Source = BlobTriggerSource.LogsAndContainerScan)] Stream waitTimesBlob, string name, CancellationToken cancellationToken)
    {
        var park = JsonSerializer.Deserialize<Park>(waitTimesBlob);

        foreach (var ride in park.Lands.SelectMany(x => x.Rides))
        {
            var seriesBlobName = ride.Name;

            var blobClient = new BlobContainerClient(_options.ConnectionString, SynchronizationOptions.SeriesBlobContainer);
            var blockBlobClient = blobClient.GetBlockBlobClient(seriesBlobName);

            RideSeries rideSeries = new()
            {
                Name = seriesBlobName,
            };

            if (await blockBlobClient.ExistsAsync(cancellationToken))
            {
                rideSeries = await LockAndRead(blockBlobClient, cancellationToken);
            }

            rideSeries.Items.Add(new RideSeriesItem
            {
                WaitTime = ride.WaitTime,
                IsOpen = ride.IsOpen,
                LastUpdated = ride.LastUpdated
            });

            rideSeries.Items = rideSeries.Items.OrderBy(x => x.LastUpdated).ToList();

            await LockAndSaveResults(rideSeries, seriesBlobName, SynchronizationOptions.SeriesBlobContainer, cancellationToken);
        }
    }

    private async Task LockAndSaveResults<TEntity>(TEntity entity, string name, string blobContainerName, CancellationToken cancellationToken)
    {
        _synchronizationLocker.Adquire(name);
        MemoryStream stream = new();
        await JsonSerializer.SerializeAsync(stream, entity, cancellationToken: cancellationToken);
        stream.Position = 0;

        var blobClient = new BlobContainerClient(_options.ConnectionString, blobContainerName);
        var blockBlobClient = blobClient.GetBlockBlobClient(name);
        if (await blockBlobClient.ExistsAsync(cancellationToken))
        {
            var writeStream = await blockBlobClient.OpenWriteAsync(true, cancellationToken: cancellationToken);
            await stream.CopyToAsync(writeStream, cancellationToken);
            await writeStream.FlushAsync(cancellationToken);
            writeStream.Close();
        }
        else
        {
            await blobClient.UploadBlobAsync(name, stream, cancellationToken);
        }

        _synchronizationLocker.Release(name);
    }

    private async Task<RideSeries> LockAndRead(BlockBlobClient client, CancellationToken cancellationToken)
    {
        _synchronizationLocker.Adquire(client.Name);
        var readStream = new MemoryStream();
        var response = await client.DownloadToAsync(readStream, cancellationToken: cancellationToken);
        readStream.Position = 0;
        var result = await JsonSerializer.DeserializeAsync<RideSeries>(readStream, cancellationToken: cancellationToken);
        _synchronizationLocker.Release(client.Name);
        return result;
    }
}
