namespace FitupProject.BLL.DTOs.AdminAccount
{
    public class AccountResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive => Status == "Active";
        public bool IsBanned => Status == "Suspended";
        public decimal PointAmount { get; set; }
        public DateTimeOffset? EmailVerifiedAt { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }

    public class AccountListResponse
    {
        public List<AccountResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
