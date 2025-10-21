namespace FSI.BusinessProcessManagement.Models.Dto
{
    public sealed class ProcessStepDto
    {
        public long Id { get; set; }
        public long ProcessId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public int StepOrder { get; set; }
        public long? AssignedRoleId { get; set; }
    }
}


