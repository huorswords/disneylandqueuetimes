using System.Collections.Concurrent;
using System.Threading;

namespace Swords.DisneyQueueTimes.Schedule.Parks;

public sealed class SynchronizationLocker
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _dictionary = new ConcurrentDictionary<string, SemaphoreSlim>();

    public void Adquire(string key)
    {
        var semaphore = _dictionary.GetOrAdd(key, new SemaphoreSlim(1, 1));
        semaphore.Wait();
    }

    public void Release(string key)
    {
        if (_dictionary.TryGetValue(key, out SemaphoreSlim semaphore))
        {
            semaphore.Release();
        }
    }
}
