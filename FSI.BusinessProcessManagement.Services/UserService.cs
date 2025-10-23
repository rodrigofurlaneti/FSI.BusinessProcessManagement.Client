using FSI.BusinessProcessManagement.Services.Http;
using FSI.BusinessProcessManagement.Models.Dto;
using FSI.BusinessProcessManagement.Models.ViewModel;

namespace FSI.BusinessProcessManagement.Services
{
    public sealed class UserService
    {
        private readonly ApiClient _api;
        public UserService(ApiClient api) => _api = api;

        public Task<List<UserDto>?> GetAllAsync()
            => _api.GetAsync<List<UserDto>>("users");

        public Task<UserDto?> GetByIdAsync(long id)
            => _api.GetAsync<UserDto>($"users/{id}");

        public Task<long> CreateAsync(UserEditVm vm)
            => _api.PostAsync<UserEditVm, long>("users", vm);

        public Task UpdateAsync(UserEditVm vm)
            => _api.PutAsync($"users/{vm.Id}", vm);

        public Task DeleteAsync(long id)
            => _api.DeleteAsync($"users/{id}");
    }
}


