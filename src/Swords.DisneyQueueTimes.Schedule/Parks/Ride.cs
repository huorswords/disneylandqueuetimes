using System.Text.Json.Serialization;

namespace Swords.DisneyQueueTimes.Schedule.Parks;

public sealed class Ride
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("is_open")]
    public bool IsOpen { get; init; }

    [JsonPropertyName("wait_time")]
    public int WaitTime { get; init; }

    [JsonPropertyName("last_updated")]
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;
}

public sealed class RideSeriesItem
{
    [JsonPropertyName("is_open")]
    public bool IsOpen { get; init; }

    [JsonPropertyName("wait_time")]
    public int WaitTime { get; init; }

    [JsonPropertyName("last_updated")]
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;
}

public sealed class RideSeries
{
    public string Name { get; set; } = string.Empty;

    public List<RideSeriesItem> Items { get; set; } = [];
}