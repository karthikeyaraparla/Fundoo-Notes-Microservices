using System.Net.Http.Json;
using Dapr.Client;
using LabelsService.Application.DTOs;
using LabelsService.Application.Interfaces;

namespace LabelsService.Infrastructure.Services;

public class DaprNoteQueryService : INoteQueryService
{
    private readonly DaprClient _daprClient;
    private readonly string _notesAppId = "notesservice";

    public DaprNoteQueryService(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<IReadOnlyList<LabeledNoteDto>> GetNotesByIdsAsync(
        string authorizationHeader,
        IReadOnlyCollection<int> noteIds,
        CancellationToken cancellationToken)
    {
        var request = _daprClient.CreateInvokeMethodRequest(
            HttpMethod.Post,
            _notesAppId,
            "api/notes/internal/by-ids" // 🔥 internal endpoint
        );

        // optional: if you want auth propagation
        if (!string.IsNullOrEmpty(authorizationHeader))
        {
            request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        }

        request.Content = JsonContent.Create(new { noteIds });

        using var response = await _daprClient.InvokeMethodWithResponseAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var notes = await response.Content.ReadFromJsonAsync<List<LabeledNoteDto>>(cancellationToken: cancellationToken);

        return notes ?? new List<LabeledNoteDto>();
    }
}