namespace Swords.DisneyQueueTimes.Schedule.Configuration;

public sealed class SynchronizationOptions
{
    public const string WaitTimesBlobContainer = "wait-times";

    public const string SeriesBlobContainer = "series";

    public string ConnectionString { get; init; } = string.Empty;
}
