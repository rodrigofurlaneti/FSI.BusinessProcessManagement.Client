namespace FSI.BusinessProcessManagement.Models.Dto
{
    public sealed class ProcessDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long? DepartmentId { get; set; }
        public string? Description { get; set; }
    }
}
