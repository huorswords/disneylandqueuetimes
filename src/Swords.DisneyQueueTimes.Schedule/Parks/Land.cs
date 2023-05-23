using System.Collections.Generic;
using System.Linq;

namespace Swords.DisneyQueueTimes.Schedule.Parks;

public sealed class Land
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public IEnumerable<Ride> Rides { get; init; } = Enumerable.Empty<Ride>();
}
