using FitupProject.Core.Commons;

namespace FitupProject.Core.Entities
{
    public class AiConversation : BaseEntity
    {
        public string AccountId { get; set; } = string.Empty;
        public string Title { get; set; } = "New chat";
        public DateTimeOffset? LastMessageAt { get; set; }

        // Dự phòng cho phase sau nếu muốn bám previous_response_id
        public string? LastOpenAIResponseId { get; set; }

        public Account? Account { get; set; }
        public ICollection<AiMessage>? Messages { get; set; }
    }
}
