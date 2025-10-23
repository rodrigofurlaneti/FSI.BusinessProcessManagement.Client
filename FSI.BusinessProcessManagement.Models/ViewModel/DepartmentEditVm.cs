using System.ComponentModel.DataAnnotations;

namespace FSI.BusinessProcessManagement.Models.ViewModel
{
    public sealed class DepartmentEditVm
    {
        public long DepartmentId { get; set; }
        [Required, MinLength(3)]
        public string DepartmentName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
