using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class PTReviewLog : BaseEntity
    {
        public string PTId { get; set; } = string.Empty;

        public PTReviewAction Action { get; set; }
        public string ActorAccountId { get; set; } = string.Empty; // user submit hoặc admin review
        public DateTimeOffset ActionAt { get; set; }

        public string? Reason { get; set; }       // reject reason / note
        public string? SnapshotJson { get; set; } // optional snapshot hồ sơ lúc action

        public PT? PT { get; set; }
    }
}
