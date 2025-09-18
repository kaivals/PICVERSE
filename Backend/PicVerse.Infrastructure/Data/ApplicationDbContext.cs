using Microsoft.EntityFrameworkCore;
using PicVerse.Core.Entities;

namespace PicVerse.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostMedia> PostMedia { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatParticipant> ChatParticipants { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<AdminAction> AdminActions { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
            
            // Post entity configuration
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany(e => e.Posts)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
            
            // PostMedia entity configuration
            modelBuilder.Entity<PostMedia>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Post)
                    .WithMany(e => e.Media)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Like entity configuration
            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.PostId }).IsUnique();
                entity.HasOne(e => e.User)
                    .WithMany(e => e.Likes)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Post)
                    .WithMany(e => e.Likes)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            
            // Comment entity configuration
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany(e => e.Comments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Post)
                    .WithMany(e => e.Comments)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.ParentComment)
                    .WithMany(e => e.Replies)
                    .HasForeignKey(e => e.ParentCommentId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            
            // CommentLike entity configuration
            modelBuilder.Entity<CommentLike>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.CommentId }).IsUnique();
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Comment)
                    .WithMany(e => e.Likes)
                    .HasForeignKey(e => e.CommentId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            
            // Follow entity configuration
            modelBuilder.Entity<Follow>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.FollowerId, e.FollowingId }).IsUnique();
                entity.HasOne(e => e.Follower)
                    .WithMany(e => e.Following)
                    .HasForeignKey(e => e.FollowerId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Following)
                    .WithMany(e => e.Followers)
                    .HasForeignKey(e => e.FollowingId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            
            // Chat entity configuration
            modelBuilder.Entity<Chat>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
            
            // ChatParticipant entity configuration
            modelBuilder.Entity<ChatParticipant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ChatId, e.UserId }).IsUnique();
                entity.HasOne(e => e.Chat)
                    .WithMany(e => e.Participants)
                    .HasForeignKey(e => e.ChatId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User)
                    .WithMany(e => e.ChatParticipants)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Message entity configuration
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Chat)
                    .WithMany(e => e.Messages)
                    .HasForeignKey(e => e.ChatId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Sender)
                    .WithMany(e => e.Messages)
                    .HasForeignKey(e => e.SenderId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
            
            // OtpCode entity configuration
            modelBuilder.Entity<OtpCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.Email, e.Code, e.Type });
                entity.HasOne(e => e.User)
                    .WithMany(e => e.OtpCodes)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
            
            // AdminAction entity configuration
            modelBuilder.Entity<AdminAction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.AdminUser)
                    .WithMany()
                    .HasForeignKey(e => e.AdminUserId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.TargetUser)
                    .WithMany()
                    .HasForeignKey(e => e.TargetUserId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.TargetPost)
                    .WithMany()
                    .HasForeignKey(e => e.TargetPostId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.TargetComment)
                    .WithMany()
                    .HasForeignKey(e => e.TargetCommentId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}