namespace FSI.BusinessProcessManagement.Models.Request
{
    public sealed class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
