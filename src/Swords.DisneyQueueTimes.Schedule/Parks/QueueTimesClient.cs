using System.Net.Http.Json;

namespace Swords.DisneyQueueTimes.Schedule.Parks;

public class QueueTimesClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    public async Task<Park?> GetQueueTimesFor(KnownParks themePark, CancellationToken cancellationToken)
        => await _httpClient.GetFromJsonAsync<Park>($"/es/parks/{(int)themePark}/queue_times.json", cancellationToken);
}
