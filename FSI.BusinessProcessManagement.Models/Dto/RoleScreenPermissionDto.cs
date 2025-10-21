namespace FSI.BusinessProcessManagement.Models.Dto
{
    public sealed class RoleScreenPermissionDto
    {
        public long Id { get; set; }
        public long RoleId { get; set; }
        public long ScreenId { get; set; }
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}


