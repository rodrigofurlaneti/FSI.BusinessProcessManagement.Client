using System.ComponentModel.DataAnnotations;

namespace FSI.BusinessProcessManagement.Models.ViewModel
{
    public sealed class UserEditVm
    {
        public long Id { get; set; }
        [Required, MinLength(3)]
        public string Username { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        public long? DepartmentId { get; set; }
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Senha inicial é obrigatória na criação")]
        public string? InitialPassword { get; set; }
    }
}
