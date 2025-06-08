using Domain.Models;
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
    
    // Users
    public DbSet<ChatUser> ChatUsers { get; set; }
    public DbSet<ChatRole> ChatRoles { get; set; }
    
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
        
        modelBuilder.Entity<ChatUser>().Navigation(ch => ch.AspNetUser).AutoInclude();
        modelBuilder.Entity<Post>().Navigation(p => p.Author).AutoInclude();
        modelBuilder.Entity<Post>().Navigation(p => p.Comments).AutoInclude();
        modelBuilder.Entity<Comment>().Navigation(c => c.Author).AutoInclude();
        modelBuilder.Entity<Comment>().Navigation(c => c.Post).AutoInclude();
        modelBuilder.Entity<Comment>().Navigation(m => m.Parent).AutoInclude();
        modelBuilder.Entity<Comment>().Navigation(a => a.Attachment).AutoInclude();
        modelBuilder.Entity<Post>().Navigation(a => a.Reactions).AutoInclude();
        
        
        base.OnModelCreating(modelBuilder);
    }
}