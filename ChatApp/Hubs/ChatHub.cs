using ChatApp.Data;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChatApp.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        public ChatHub(AppDbContext context) => _context = context;

        // --- 1. CHAT RIÊNG (1-1) ---
        public async Task SendPrivateMessage(string targetUserId, string message)
        {
            try
            {
                // A. Lấy thông tin người gửi an toàn
                var senderName = Context.User?.Identity?.Name;
                var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Kiểm tra nếu Token bị lỗi
                if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(senderName))
                {
                    Console.WriteLine("❌ Lỗi: Không tìm thấy ID hoặc Tên trong Token");
                    return; // Dừng lại, không chạy tiếp
                }

                Console.WriteLine($"DEBUG: {senderName} (ID: {senderId}) đang gửi tin cho {targetUserId}");

                // B. Lưu vào Database
                var msg = new Message
                {
                    SenderName = senderName,
                    Content = message,
                    ReceiverId = targetUserId,
                    GroupName = null, // Đặt rõ ràng là null
                    CreatedAt = DateTime.UtcNow // Đảm bảo có thời gian
                };

                _context.Messages.Add(msg);
                await _context.SaveChangesAsync(); // <-- Nếu lỗi DB, nó sẽ nhảy xuống catch

                // C. Gửi Realtime
                // Gửi cho người nhận
                await Clients.User(targetUserId).SendAsync("ReceivePrivate", senderId, senderName, message);

                // Gửi lại cho chính mình (để hiện bên phải màn hình chat)
                await Clients.Caller.SendAsync("ReceivePrivate", senderId, "Tôi", message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LỖI NGHIÊM TRỌNG TRONG HUB: {ex.Message}");
                Console.WriteLine(ex.StackTrace); // In chi tiết lỗi ra màn hình đen
                throw; // Ném lỗi ra để Client biết
            }
        }

        // --- 2. THAM GIA PHÒNG ---
        public async Task JoinRoom(string roomName)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                await Clients.Group(roomName).SendAsync("ReceiveSystemMessage", $"{Context.User.Identity.Name} đã tham gia phòng {roomName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi JoinRoom: {ex.Message}");
            }
        }

        // --- 3. CHAT NHÓM ---
        public async Task SendRoomMessage(string roomName, string message)
        {
            try
            {
                var senderName = Context.User.Identity.Name;

                _context.Messages.Add(new Message
                {
                    SenderName = senderName,
                    Content = message,
                    GroupName = roomName,
                    ReceiverId = null, // Chat nhóm thì không có người nhận cụ thể
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                await Clients.Group(roomName).SendAsync("ReceiveRoom", roomName, senderName, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi SendRoomMessage: {ex.Message}");
                throw;
            }
        }
    }
}