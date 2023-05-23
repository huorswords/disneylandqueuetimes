using System.Collections.Generic;
using System.Linq;

namespace Swords.DisneyQueueTimes.Schedule.Parks;

public sealed class Park
{
    public IEnumerable<Land> Lands { get; init; } = Enumerable.Empty<Land>();
}
