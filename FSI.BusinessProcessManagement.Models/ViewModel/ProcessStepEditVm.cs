using System.ComponentModel.DataAnnotations;

namespace FSI.BusinessProcessManagement.Models.ViewModel
{
    public sealed class ProcessStepEditVm
    {
        public long StepId { get; set; }
        public long ProcessId { get; set; }
        [Required, MinLength(3)]
        public string? StepName { get; set; }
        public int StepOrder { get; set; }
        public long? AssignedRoleId { get; set; }
    }
}
