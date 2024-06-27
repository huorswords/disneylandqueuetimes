namespace Swords.DisneyQueueTimes.Schedule.Parks;

public sealed class Park
{
    public IEnumerable<Land> Lands { get; init; } = Enumerable.Empty<Land>();

    public IEnumerable<Ride> Rides { get; init; } = Enumerable.Empty<Ride>();
}
