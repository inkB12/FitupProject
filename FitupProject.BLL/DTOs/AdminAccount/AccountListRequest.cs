namespace FitupProject.BLL.DTOs.AdminAccount
{
    public class AccountListRequest
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? Role { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
