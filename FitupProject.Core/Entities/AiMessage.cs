using FitupProject.Core.Commons;
using FitupProject.Core.Commons.Enums;

namespace FitupProject.Core.Entities
{
    public class AiMessage : BaseEntity
    {
        public string ConversationId { get; set; } = string.Empty;
        public AiMessageRole Role { get; set; } = AiMessageRole.User;
        public string Content { get; set; } = string.Empty;

        // usage của riêng lần AI trả lời
        public int? InputTokens { get; set; }
        public int? OutputTokens { get; set; }
        public int? TotalTokens { get; set; }

        public string? OpenAIResponseId { get; set; }

        public AiConversation? Conversation { get; set; }
    }
}
