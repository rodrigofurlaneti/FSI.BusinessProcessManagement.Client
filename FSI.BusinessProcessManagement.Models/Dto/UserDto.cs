namespace FSI.BusinessProcessManagement.Models.Dto
{
    public sealed class UserDto
    {
        public long UserId { get; set; }
        public long? DepartmentId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;
        public string? PasswordHash { get; set; }
    }
}
