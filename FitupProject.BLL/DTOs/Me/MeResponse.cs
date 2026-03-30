namespace FitupProject.BLL.DTOs.Me
{
    public class MeResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal PointAmount { get; set; }

        public MeProfileResponse? Profile { get; set; }
    }

    public class MeProfileResponse
    {
        public string? FullName { get; set; }
        public DateTime? Dob { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
    }
}
