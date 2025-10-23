using System.ComponentModel.DataAnnotations;

namespace FSI.BusinessProcessManagement.Models.ViewModel
{
    public sealed class RoleCreateVm
    {
        [Required, MinLength(3)]
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
