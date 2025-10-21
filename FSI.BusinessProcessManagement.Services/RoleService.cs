using FSI.BusinessProcessManagement.Services.Http;
using FSI.BusinessProcessManagement.Models.Dto;

namespace FSI.BusinessProcessManagement.Services;

public sealed class RoleService
{
    private readonly ApiClient _api;
    public RoleService(ApiClient api) => _api = api;

    public Task<List<RoleDto>?> GetAllAsync()
        => _api.GetAsync<List<RoleDto>>("/api/role");
}