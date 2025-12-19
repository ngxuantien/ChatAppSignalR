using ChatApp.Data;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;

namespace ChatApp.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        public ChatHub(AppDbContext context) => _context = context;

        public async Task SendMessage(string content)
        {
            var userName = Context.User?.Identity?.Name ?? "Anonymous";

            var msg = new Message { 
                SenderName = userName, 
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            // Gửi cho tất cả mọi người
            await Clients.All.SendAsync("ReceiveMessage", userName, content);
        }
    }
}
