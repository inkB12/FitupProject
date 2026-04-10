using System.Security.Claims;
using FitupProject.BLL.DTOs.AIChat;
using FitupProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("api/ai-chat")]
    [Authorize]
    public class AIChatController : ControllerBase
    {
        private readonly IAIChatService _aiChatService;

        public AIChatController(IAIChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        private string? GetAccountId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
        }

        [HttpPost("conversations")]
        public async Task<IActionResult> CreateConversation([FromBody] CreateAiConversationRequest request)
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId)) return Unauthorized();

            var data = await _aiChatService.CreateConversationAsync(accountId, request);
            return Ok(data);
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetMyConversations()
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId)) return Unauthorized();

            var data = await _aiChatService.GetMyConversationsAsync(accountId);
            return Ok(data);
        }

        [HttpGet("conversations/{conversationId}")]
        public async Task<IActionResult> GetConversationDetail(string conversationId)
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId)) return Unauthorized();

            var data = await _aiChatService.GetConversationDetailAsync(accountId, conversationId);
            return Ok(data);
        }

        [HttpPost("conversations/{conversationId}/messages")]
        public async Task<IActionResult> SendMessage(string conversationId, [FromBody] SendAiMessageRequest request)
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId)) return Unauthorized();

            var data = await _aiChatService.SendMessageAsync(accountId, conversationId, request);
            return Ok(data);
        }

        [HttpDelete("conversations/{conversationId}")]
        public async Task<IActionResult> DeleteConversation(string conversationId)
        {
            var accountId = GetAccountId();
            if (string.IsNullOrWhiteSpace(accountId)) return Unauthorized();

            await _aiChatService.DeleteConversationAsync(accountId, conversationId);
            return Ok(new { message = "Đã xóa cuộc trò chuyện." });
        }
    }
}
