using System.Globalization;
using Dapr.Client;
using NotesService.Application.Interfaces;

namespace NotesService.Infrastructure.Cache;

public class DaprStateCacheService : ICacheService
{
    private readonly DaprClient _client;
    private readonly string _stateStoreName;

    public DaprStateCacheService(DaprClient client, string stateStoreName)
    {
        _client = client;
        _stateStoreName = stateStoreName;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        return await _client.GetStateAsync<T>(_stateStoreName, key);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        Dictionary<string, string>? metadata = null;

        if (expiry.HasValue)
        {
            metadata = new Dictionary<string, string>
            {
                ["ttlInSeconds"] = Math.Max(1, (int)Math.Ceiling(expiry.Value.TotalSeconds))
                    .ToString(CultureInfo.InvariantCulture)
            };
        }

        return _client.SaveStateAsync(_stateStoreName, key, value, metadata: metadata);
    }

    public Task RemoveAsync(string key)
    {
        return _client.DeleteStateAsync(_stateStoreName, key);
    }
}
