using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Swords.DisneyQueueTimes.Schedule.Parks;

public class QueueTimesClient
{
    private readonly HttpClient _httpClient;

    public QueueTimesClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<Park> GetQueueTimesFor(KnownParks themePark, CancellationToken cancellationToken)
        => await _httpClient.GetFromJsonAsync<Park>($"/es/parks/{(int)themePark}/queue_times.json", cancellationToken);
}
