using System.ComponentModel.DataAnnotations;

namespace PicVerse.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        [StringLength(255)]
        public string? FirstName { get; set; }
        
        [StringLength(255)]
        public string? LastName { get; set; }
        
        [StringLength(500)]
        public string? Bio { get; set; }
        
        [StringLength(255)]
        public string? ProfilePictureUrl { get; set; }
        
        [StringLength(255)]
        public string? CoverPictureUrl { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        
        public bool IsEmailVerified { get; set; } = false;
        
        public bool IsPhoneVerified { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public bool IsPrivate { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Follow> Followers { get; set; } = new List<Follow>();
        public virtual ICollection<Follow> Following { get; set; } = new List<Follow>();
        public virtual ICollection<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public virtual ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();
    }
}