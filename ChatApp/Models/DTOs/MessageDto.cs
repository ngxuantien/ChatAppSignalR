namespace ChatApp.Models.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
