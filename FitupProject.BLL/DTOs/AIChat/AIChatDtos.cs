namespace FitupProject.BLL.DTOs.AIChat
{
    public class CreateAiConversationRequest
    {
        public string? Title { get; set; }
    }

    public class AiConversationListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? LastMessageAt { get; set; }
    }

    public class AiMessageDto
    {
        public string Id { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTimeOffset? CreatedAt { get; set; }
        public int? InputTokens { get; set; }
        public int? OutputTokens { get; set; }
        public int? TotalTokens { get; set; }
    }

    public class AiConversationDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? LastMessageAt { get; set; }
        public List<AiMessageDto> Messages { get; set; } = new();
    }

    public class SendAiMessageRequest
    {
        public string Message { get; set; } = string.Empty;

        // phase 2 toggle
        public bool IncludeLiveContext { get; set; } = true;
    }

    public class SendAiMessageResponse
    {
        public string ConversationId { get; set; } = string.Empty;
        public AiMessageDto UserMessage { get; set; } = new();
        public AiMessageDto AssistantMessage { get; set; } = new();
    }
}
