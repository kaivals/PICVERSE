using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PicVerse.API.Services.Interfaces;
using System.Security.Claims;

namespace PicVerse.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            
            // Update user online status
            await Clients.All.SendAsync("UserOnline", userId);
            
            _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            
            // Update user offline status
            await Clients.All.SendAsync("UserOffline", userId);
            
            _logger.LogInformation("User {UserId} disconnected", userId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChat(int chatId)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // Verify user has access to this chat
                var chat = await _chatService.GetChatByIdAsync(chatId, userId);
                if (chat != null)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
                    await Clients.Caller.SendAsync("JoinedChat", chatId);
                    
                    _logger.LogInformation("User {UserId} joined chat {ChatId}", userId, chatId);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Access denied to chat");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining chat {ChatId}", chatId);
                await Clients.Caller.SendAsync("Error", "Failed to join chat");
            }
        }

        public async Task LeaveChat(int chatId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
                await Clients.Caller.SendAsync("LeftChat", chatId);
                
                _logger.LogInformation("User {UserId} left chat {ChatId}", userId, chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving chat {ChatId}", chatId);
            }
        }

        public async Task SendMessage(int chatId, string content, string messageType = "text")
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var message = await _chatService.SendMessageAsync(chatId, userId, new DTOs.Chats.SendMessageRequest
                {
                    Content = content,
                    Type = messageType
                });

                // Send message to all chat participants
                await Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessage", new
                {
                    id = message.Id,
                    chatId = message.ChatId,
                    senderId = message.SenderId,
                    senderName = message.SenderName,
                    senderAvatar = message.SenderAvatar,
                    content = message.Content,
                    type = message.Type,
                    createdAt = message.CreatedAt
                });

                _logger.LogInformation("Message sent by user {UserId} to chat {ChatId}", userId, chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to chat {ChatId}", chatId);
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }

        public async Task TypingStart(int chatId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await Clients.GroupExcept($"chat_{chatId}", Context.ConnectionId)
                    .SendAsync("UserTyping", chatId, userId, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending typing start for chat {ChatId}", chatId);
            }
        }

        public async Task TypingStop(int chatId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await Clients.GroupExcept($"chat_{chatId}", Context.ConnectionId)
                    .SendAsync("UserTyping", chatId, userId, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending typing stop for chat {ChatId}", chatId);
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = Context.User?.FindFirst("userId")?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}