using FSI.BusinessProcessManagement.Models.Dto;
using FSI.BusinessProcessManagement.Models.ViewModel;
using FSI.BusinessProcessManagement.Services.Http;

namespace FSI.BusinessProcessManagement.Services
{
    public sealed class DepartmentService
    {
        private readonly ApiClient _api;
        public DepartmentService(ApiClient api) => _api = api;

        public Task<List<DepartmentDto>?> GetAllAsync()
            => _api.GetAsync<List<DepartmentDto>>("departments");

        public async Task<DepartmentDto?> GetByIdAsync(long id)
        {
            var result = await _api.GetAsync<DepartmentDto>($"departments/{id}");
            if (result is null) return null;
            return result;
        }

        public async Task<long> CreateAsync(DepartmentCreateVm vm)
        {
            if (string.IsNullOrWhiteSpace(vm.DepartmentName))
                throw new ArgumentException("Nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(vm.Description))
                throw new ArgumentException("Descrição é obrigatório.");

            var payload = new DepartmentDto
            {
                DepartmentName = vm.DepartmentName,
                Description = vm.Description
            };

            var result = await _api.PostAsync<DepartmentDto, long>("departments", payload);
            return result;
        }

        public async Task UpdateAsync(DepartmentEditVm vm)
        {
            if (vm.DepartmentId <= 0) throw new ArgumentException("Id inválido.");

            if (string.IsNullOrWhiteSpace(vm.DepartmentName))
                throw new ArgumentException("Nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(vm.Description))
                throw new ArgumentException("Descrição é obrigatório.");

            var payload = new DepartmentDto
            {
                DepartmentId = vm.DepartmentId,
                DepartmentName = vm.DepartmentName,
                Description = vm.Description
            };

            await _api.PutAsync($"departments/{payload.DepartmentId}", payload);
        }

        public Task DeleteAsync(long id)
            => _api.DeleteAsync($"departments/{id}");
    }
}


