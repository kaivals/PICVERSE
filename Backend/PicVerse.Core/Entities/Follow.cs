namespace PicVerse.Core.Entities
{
    public class Follow
    {
        public int Id { get; set; }
        
        public int FollowerId { get; set; }
        
        public int FollowingId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User Follower { get; set; } = null!;
        public virtual User Following { get; set; } = null!;
    }
}