using System.ComponentModel.DataAnnotations;

namespace PicVerse.Core.Entities
{
    public class AdminAction
    {
        public int Id { get; set; }
        
        public int AdminUserId { get; set; }
        
        public int? TargetUserId { get; set; }
        
        public int? TargetPostId { get; set; }
        
        public int? TargetCommentId { get; set; }
        
        public AdminActionType ActionType { get; set; }
        
        [StringLength(1000)]
        public string? Reason { get; set; }
        
        [StringLength(2000)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User AdminUser { get; set; } = null!;
        public virtual User? TargetUser { get; set; }
        public virtual Post? TargetPost { get; set; }
        public virtual Comment? TargetComment { get; set; }
    }
    
    public enum AdminActionType
    {
        UserSuspend = 1,
        UserBan = 2,
        UserUnban = 3,
        PostDelete = 4,
        PostHide = 5,
        PostRestore = 6,
        CommentDelete = 7,
        CommentHide = 8,
        CommentRestore = 9,
        UserWarning = 10
    }
}