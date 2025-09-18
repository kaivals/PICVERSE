using System.ComponentModel.DataAnnotations;

namespace PicVerse.Core.Entities
{
    public class OtpCode
    {
        public int Id { get; set; }
        
        public int? UserId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        
        public OtpType Type { get; set; }
        
        public bool IsUsed { get; set; } = false;
        
        public DateTime ExpiresAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User? User { get; set; }
    }
    
    public enum OtpType
    {
        Login = 1,
        Registration = 2,
        PasswordReset = 3,
        EmailVerification = 4,
        PhoneVerification = 5
    }
}