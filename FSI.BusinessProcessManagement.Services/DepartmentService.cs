using System.Net.Http.Json;
using FSI.BusinessProcessManagement.Services.Http;
using FSI.BusinessProcessManagement.Models.Dto;

namespace FSI.BusinessProcessManagement.Services;

public sealed class DepartmentService
{
    private readonly ApiClient _api;

    public DepartmentService(ApiClient api) => _api = api;

    public Task<List<DepartmentDto>> GetAllAsync()
        => _api.GetAsync<List<DepartmentDto>>("api/departments")!;

    public Task<DepartmentDto?> GetByIdAsync(long id)
        => _api.GetAsync<DepartmentDto>($"api/departments/{id}");

    public Task<long> CreateAsync(DepartmentDto dto)
        => _api.PostAsync<DepartmentDto, long>("api/departments", dto);

    public Task UpdateAsync(DepartmentDto dto)
        => _api.PutAsync("api/departments/{dto.Id}", dto);

    public Task DeleteAsync(long id)
        => _api.DeleteAsync($"api/departments/{id}");
}