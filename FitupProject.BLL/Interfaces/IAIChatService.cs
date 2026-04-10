using FitupProject.BLL.DTOs.AIChat;

namespace FitupProject.BLL.Interfaces
{
    public interface IAIChatService
    {
        Task<AiConversationListItemDto> CreateConversationAsync(string accountId, CreateAiConversationRequest request);
        Task<List<AiConversationListItemDto>> GetMyConversationsAsync(string accountId);
        Task<AiConversationDetailDto> GetConversationDetailAsync(string accountId, string conversationId);
        Task<SendAiMessageResponse> SendMessageAsync(string accountId, string conversationId, SendAiMessageRequest request);
        Task DeleteConversationAsync(string accountId, string conversationId);
    }
}
