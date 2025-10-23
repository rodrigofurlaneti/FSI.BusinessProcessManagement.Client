using System.ComponentModel.DataAnnotations;

namespace FSI.BusinessProcessManagement.Models.ViewModel
{
    public sealed class RoleEditVm
    {
        public long RoleId { get; set; }
        [Required, MinLength(3)]
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
