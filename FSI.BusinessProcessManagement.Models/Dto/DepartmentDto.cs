namespace FSI.BusinessProcessManagement.Models.Dto
{
    public sealed class DepartmentDto
    {
        public long DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
