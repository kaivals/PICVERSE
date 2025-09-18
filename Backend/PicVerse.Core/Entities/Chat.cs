using System.ComponentModel.DataAnnotations;

namespace PicVerse.Core.Entities
{
    public class Chat
    {
        public int Id { get; set; }
        
        [StringLength(255)]
        public string? Name { get; set; }
        
        public bool IsGroup { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
    
    public class ChatParticipant
    {
        public int Id { get; set; }
        
        public int ChatId { get; set; }
        
        public int UserId { get; set; }
        
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastReadAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual Chat Chat { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}