using System.Net.Http.Json;
using FSI.BusinessProcessManagement.Services.Http;
using FSI.BusinessProcessManagement.Models.Dto;

namespace FSI.BusinessProcessManagement.Services;

public sealed class ProcessService
{
    private readonly ApiClient _api;

    public ProcessService(ApiClient api)
    {
        _api = api;
    }

    public async Task<IReadOnlyList<ProcessDto>> GetAllAsync(CancellationToken ct = default)
        => await _api.GetAsync<List<ProcessDto>>("api/process") ?? new List<ProcessDto>();

    public async Task<long> CreateAsync(string name, long? departmentId, string? description, CancellationToken ct = default)
    {
        var body = new
        {
            name,
            departmentId,
            description
        };

        var result = await _api.PostAsync<object, long>("api/process", body, ct);
        return result;
    }
}
