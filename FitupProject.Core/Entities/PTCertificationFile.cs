using FitupProject.Core.Commons;

namespace FitupProject.Core.Entities
{
    public class PTCertificationFile : BaseEntity
    {
        public string PTId { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty; // URL sau khi FE upload lên storage
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }

        public DateTimeOffset UploadedAt { get; set; }

        public PT? PT { get; set; }
    }
}
