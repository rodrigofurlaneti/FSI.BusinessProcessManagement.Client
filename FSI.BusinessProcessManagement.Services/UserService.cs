using BCrypt.Net;
using FSI.BusinessProcessManagement.Models.Dto;
using FSI.BusinessProcessManagement.Models.ViewModel;
using FSI.BusinessProcessManagement.Services.Http;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace FSI.BusinessProcessManagement.Services
{
    public sealed class UserService
    {
        private readonly ApiClient _api;
        public UserService(ApiClient api) => _api = api;

        public Task<List<UserDto>?> GetAllAsync()
            => _api.GetAsync<List<UserDto>>("users");

        public async Task<UserDto?> GetByIdAsync(long id)
        {
            var result = await _api.GetAsync<UserDto>($"users/{id}");
            if (result is null) return null;
            result.PasswordHash = null;
            return result;
        }

        public async Task<long> CreateAsync(UserCreateVm vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Username))
                throw new ArgumentException("Username é obrigatório.");
            if (string.IsNullOrWhiteSpace(vm.PasswordHash))
                throw new ArgumentException("Senha inicial é obrigatória.");

            var hash = BCrypt.Net.BCrypt.HashPassword(vm.PasswordHash, workFactor: 12);

            var payload = new UserDto
            {
                Username = vm.Username,
                Email = vm.Email,
                DepartmentId = vm.DepartmentId,
                IsActive = vm.IsActive,
                PasswordHash = hash
            };

            var result = await _api.PostAsync<UserDto, long>("users", payload);
            return result;
        }

        public async Task UpdateAsync(UserEditVm vm)
        {
            if (vm.UserId <= 0) throw new ArgumentException("Id inválido.");

            string? hash = null;
            if (!string.IsNullOrWhiteSpace(vm.PasswordHash))
                hash = BCrypt.Net.BCrypt.HashPassword(vm.PasswordHash, workFactor: 12);

            var payload = new UserDto
            {
                UserId = vm.UserId,
                Username = vm.Username,
                Email = vm.Email,
                DepartmentId = vm.DepartmentId,
                IsActive = vm.IsActive,
                PasswordHash = hash 
            };

            await _api.PutAsync($"users/{payload.UserId}", payload);
        }

        public Task DeleteAsync(long id)
            => _api.DeleteAsync($"users/{id}");
    }
}


