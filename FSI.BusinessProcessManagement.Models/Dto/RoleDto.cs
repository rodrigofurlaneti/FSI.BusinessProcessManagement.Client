namespace FSI.BusinessProcessManagement.Models.Dto
{
    public sealed class RoleDto
    {
        public long RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
