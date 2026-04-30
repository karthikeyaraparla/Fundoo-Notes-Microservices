using Dapr.Client;

namespace LabelsService.Infrastructure.Services;

public class DaprStateCacheService
{
    private readonly DaprClient _daprClient;
    private const string StoreName = "statestore";

    public DaprStateCacheService(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        return await _daprClient.GetStateAsync<T>(StoreName, key);
    }

    public async Task SetAsync<T>(string key, T value)
    {
        await _daprClient.SaveStateAsync(StoreName, key, value);
    }

    public async Task DeleteAsync(string key)
    {
        await _daprClient.DeleteStateAsync(StoreName, key);
    }
}