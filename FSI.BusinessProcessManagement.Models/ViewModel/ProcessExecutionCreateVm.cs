namespace FSI.BusinessProcessManagement.Models.ViewModel
{
    public sealed class ProcessExecutionCreateVm
    {
        public long ProcessId { get; set; }
        public long StepId { get; set; }
        public long? UserId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Remarks { get; set; }
    }
}
