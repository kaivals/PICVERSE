using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PicVerse.API.DTOs.Chats;
using PicVerse.API.Services.Interfaces;

namespace PicVerse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatsController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatsController> _logger;

        public ChatsController(IChatService chatService, ILogger<ChatsController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetChats([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                var chats = await _chatService.GetUserChatsAsync(userId, page, pageSize);
                
                return Ok(chats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chats for user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var chat = await _chatService.CreateChatAsync(userId, request);
                
                return Ok(chat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChat(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var chat = await _chatService.GetChatByIdAsync(id, userId);
                
                if (chat == null)
                {
                    return NotFound(new { message = "Chat not found" });
                }
                
                return Ok(chat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat {ChatId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}/messages")]
        public async Task<IActionResult> GetMessages(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var userId = GetCurrentUserId();
                var messages = await _chatService.GetMessagesAsync(id, userId, page, pageSize);
                
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for chat {ChatId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("{id}/messages")]
        public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var message = await _chatService.SendMessageAsync(id, userId, request);
                
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to chat {ChatId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("messages/{messageId}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int messageId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _chatService.MarkMessageAsReadAsync(messageId, userId);
                
                if (result.Success)
                {
                    return Ok(new { message = "Message marked as read" });
                }
                
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message {MessageId} as read", messageId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _chatService.DeleteMessageAsync(messageId, userId);
                
                if (result.Success)
                {
                    return Ok(new { message = "Message deleted successfully" });
                }
                
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}