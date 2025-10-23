using System.ComponentModel.DataAnnotations;

namespace FSI.BusinessProcessManagement.Models.ViewModel
{
    public sealed class DepartmentCreateVm
    {
        [Required, MinLength(3)]
        public string DepartmentName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
