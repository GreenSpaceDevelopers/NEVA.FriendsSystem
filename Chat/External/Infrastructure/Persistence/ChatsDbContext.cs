using Domain.Models.Blog;
using Domain.Models.Media;
using Domain.Models.Messaging;
using Domain.Models.Service;
using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ChatsDbContext(DbContextOptions<ChatsDbContext> options) : DbContext(options)
{
    // Messaging
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Reaction> Reactions { get; set; }
    public DbSet<ReactionType> ReactionTypes { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<AttachmentType> AttachmentTypes { get; set; }
    public DbSet<UserChatSettings> UserChatSettings { get; set; }

    // Users
    public DbSet<ChatUser> ChatUsers { get; set; }
    public DbSet<ChatRole> ChatRoles { get; set; }
    public DbSet<UserPrivacySettings> UserPrivacySettings { get; set; }
    public DbSet<NotificationSettings> NotificationSettings { get; set; }

    // MediaDto
    public DbSet<Picture> Services { get; set; }

    // Service
    public DbSet<Event> Events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Chat>()
            .HasOne(c => c.RelatedEvent);

        modelBuilder
            .Entity<Chat>()
            .HasOne(c => c.ChatPicture);

        modelBuilder
            .Entity<Chat>()
            .HasMany(c => c.Messages)
            .WithOne(c => c.Chat);

        modelBuilder.Entity<Chat>()
            .HasMany(c => c.Users)
            .WithMany(u => u.Chats)
            .UsingEntity(j => j.ToTable("ChatUserChats"));

        modelBuilder.Entity<Chat>()
            .HasOne(c => c.Admin)
            .WithMany()
            .HasForeignKey(c => c.AdminId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure ChatUser many-to-many relationships
        modelBuilder.Entity<ChatUser>()
            .HasMany(u => u.Friends)
            .WithMany()
            .UsingEntity(j => j.ToTable("UserFriends"));

        modelBuilder.Entity<ChatUser>()
            .HasMany(u => u.FriendRequests)
            .WithMany(u => u.WaitingFriendRequests)
            .UsingEntity(j => j.ToTable("FriendRequests"));

        modelBuilder.Entity<ChatUser>()
            .HasMany(u => u.BlockedUsers)
            .WithMany()
            .UsingEntity(j => j.ToTable("UserBlockedUsers"));

        // Configure ChatUser foreign keys
        modelBuilder.Entity<ChatUser>()
            .HasOne(u => u.Avatar)
            .WithMany()
            .HasForeignKey(u => u.AvatarId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ChatUser>()
            .HasOne(u => u.Cover)
            .WithMany()
            .HasForeignKey(u => u.CoverId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ChatUser>()
            .HasOne(u => u.PrivacySettings)
            .WithOne(p => p.ChatUser)
            .HasForeignKey<UserPrivacySettings>(p => p.ChatUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatUser>().Navigation(ch => ch.AspNetUser).AutoInclude();
        modelBuilder.Entity<Post>().Navigation(p => p.Author).AutoInclude();
        modelBuilder.Entity<Post>().Navigation(p => p.Comments).AutoInclude();
        modelBuilder.Entity<Comment>().Navigation(c => c.Author).AutoInclude();
        modelBuilder.Entity<Comment>().Navigation(c => c.Post).AutoInclude();
        // modelBuilder.Entity<Comment>().Navigation(m => m.Parent).AutoInclude();
        modelBuilder.Entity<Comment>().Navigation(a => a.Attachment).AutoInclude();
        modelBuilder.Entity<Post>().Navigation(a => a.Reactions).AutoInclude();
        
        modelBuilder.Entity<ChatUser>()
            .HasOne(u => u.NotificationSettings)
            .WithOne(n => n.User)
            .HasForeignKey<NotificationSettings>(n  => n.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Parent)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure UserChatSettings relationships
        modelBuilder.Entity<UserChatSettings>()
            .HasOne(ucs => ucs.User)
            .WithMany(u => u.ChatSettings)
            .HasForeignKey(ucs => ucs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserChatSettings>()
            .HasOne(ucs => ucs.Chat)
            .WithMany()
            .HasForeignKey(ucs => ucs.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure UserChatSettings many-to-many with ChatUser for DisabledUsers
        modelBuilder.Entity<UserChatSettings>()
            .HasMany(ucs => ucs.DisabledUsers)
            .WithMany()
            .UsingEntity(
                "UserChatSettingsDisabledUsers",
                l => l.HasOne(typeof(ChatUser)).WithMany().HasForeignKey("DisabledUserId"),
                r => r.HasOne(typeof(UserChatSettings)).WithMany().HasForeignKey("UserChatSettingsId")
            );

        base.OnModelCreating(modelBuilder);
    }
}