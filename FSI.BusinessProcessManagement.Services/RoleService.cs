using FSI.BusinessProcessManagement.Models.Dto;
using FSI.BusinessProcessManagement.Models.ViewModel;
using FSI.BusinessProcessManagement.Services.Http;

namespace FSI.BusinessProcessManagement.Services
{
    public sealed class RoleService
    {
        private readonly ApiClient _api;
        public RoleService(ApiClient api) => _api = api;

        public Task<List<RoleDto>?> GetAllAsync()
            => _api.GetAsync<List<RoleDto>>("roles");

        public async Task<RoleDto?> GetByIdAsync(long id)
        {
            var result = await _api.GetAsync<RoleDto>($"roles/{id}");
            if (result is null) return null;
            return result;
        }

        public async Task<long> CreateAsync(RoleCreateVm vm)
        {
            if (string.IsNullOrWhiteSpace(vm.RoleName))
                throw new ArgumentException("Nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(vm.Description))
                throw new ArgumentException("Descrição é obrigatório.");

            var payload = new RoleDto
            {
                RoleName = vm.RoleName,
                Description = vm.Description
            };

            var result = await _api.PostAsync<RoleDto, long>("roles", payload);
            return result;
        }

        public async Task UpdateAsync(RoleEditVm vm)
        {
            if (vm.RoleId <= 0) throw new ArgumentException("Id inválido.");

            if (string.IsNullOrWhiteSpace(vm.RoleName))
                throw new ArgumentException("Nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(vm.Description))
                throw new ArgumentException("Descrição é obrigatório.");

            var payload = new RoleDto
            {
                RoleId = vm.RoleId,
                RoleName = vm.RoleName,
                Description = vm.Description
            };

            await _api.PutAsync($"roles/{payload.RoleId}", payload);
        }

        public Task DeleteAsync(long id)
            => _api.DeleteAsync($"roles/{id}");
    }
}


