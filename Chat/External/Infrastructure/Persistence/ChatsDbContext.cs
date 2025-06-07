using Domain.Models;
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

        // 👤 Один-ко-многим: Chat.Admin ↔ ChatUser
        modelBuilder.Entity<Chat>()
            .HasOne(c => c.Admin)
            .WithMany() // без навигации обратно!
            .HasForeignKey(c => c.AdminId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        base.OnModelCreating(modelBuilder);
    }
}