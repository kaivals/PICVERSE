using System.ComponentModel.DataAnnotations;

namespace PicVerse.Core.Entities
{
    public class Message
    {
        public int Id { get; set; }
        
        public int ChatId { get; set; }
        
        public int SenderId { get; set; }
        
        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;
        
        public MessageType Type { get; set; } = MessageType.Text;
        
        [StringLength(500)]
        public string? MediaUrl { get; set; }
        
        public bool IsRead { get; set; } = false;
        
        public bool IsEdited { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Chat Chat { get; set; } = null!;
        public virtual User Sender { get; set; } = null!;
    }
    
    public enum MessageType
    {
        Text = 1,
        Image = 2,
        Video = 3,
        File = 4,
        Audio = 5
    }
}