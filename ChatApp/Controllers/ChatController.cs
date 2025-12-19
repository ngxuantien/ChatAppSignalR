using ChatApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public ChatController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            if (idClaim == null)
            {
                return Unauthorized("Token không hợp lệ: Không tìm thấy User ID.");
            }
            if (!int.TryParse(idClaim.Value, out int currentUserId))
            {
                return BadRequest("User ID trong token không phải là số.");
            }
            var users = _context.Users
                .Where(u => u.Id != currentUserId)
                .Select(u => new { u.Id, u.Username })
                .ToList();
            return Ok(users);
        }
    }
}
