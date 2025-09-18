namespace PicVerse.Core.Entities
{
    public class Like
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public int PostId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Post Post { get; set; } = null!;
    }
}