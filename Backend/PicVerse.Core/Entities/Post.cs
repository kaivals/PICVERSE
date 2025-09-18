using System.ComponentModel.DataAnnotations;

namespace PicVerse.Core.Entities
{
    public class Post
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        [StringLength(2000)]
        public string? Content { get; set; }
        
        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        [StringLength(500)]
        public string? VideoUrl { get; set; }
        
        public int LikesCount { get; set; } = 0;
        
        public int CommentsCount { get; set; } = 0;
        
        public int SharesCount { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<PostMedia> Media { get; set; } = new List<PostMedia>();
    }
    
    public class PostMedia
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        
        [Required]
        [StringLength(500)]
        public string MediaUrl { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string MediaType { get; set; } = string.Empty; // image, video
        
        public int Order { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Post Post { get; set; } = null!;
    }
}