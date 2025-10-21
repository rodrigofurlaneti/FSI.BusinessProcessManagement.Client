using FSI.BusinessProcessManagement.Services.Http;
using FSI.BusinessProcessManagement.Models.Dto;

namespace FSI.BusinessProcessManagement.Services;

public sealed class UserService
{
    private readonly ApiClient _api;
    public UserService(ApiClient api) => _api = api;

    public Task<List<UserDto>?> GetAllAsync()
        => _api.GetAsync<List<UserDto>>("api/usuario");

    public Task<UserDto?> GetByIdAsync(long id)
        => _api.GetAsync<UserDto>($"api/usuario/{id}");
}