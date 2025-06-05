using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Dtos.Requests.Shared;
using Domain.Models.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Messaging;

public class ChatsRepository(ChatsDbContext dbContext) : BaseRepository<Chat>(dbContext), IChatsRepository
{
    public async Task<List<Chat>> GetUserChatsNoTrackingAsync(Guid userId, PageSettings pageSettings, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Chat>()
            .AsNoTracking()
            .Include(c => c.ChatPicture)
            .Where(c => c.Users.Any(u => u.Id == userId))
            .OrderByDescending(c => c.LastMessageDate)
            .Skip((pageSettings.PageNumber - 1) * pageSettings.PageSize)
            .Take(pageSettings.PageSize)
            .Select(chat => new Chat
            {
                Id = chat.Id,
                Name = chat.Name,
                ChatPictureId = chat.ChatPictureId,
                AdminId = chat.AdminId,
                RelatedEventId = chat.RelatedEventId,
                LastMessageDate = chat.LastMessageDate,

                ChatPicture = chat.ChatPicture,

                Messages = chat.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new Message
                    {
                        Id = m.Id,
                        ChatId = m.ChatId,
                        SenderId = m.SenderId,
                        AttachmentId = m.AttachmentId,
                        Content = m.Content,
                        CreatedAt = m.CreatedAt,
                        Sender = m.Sender,
                        Attachment = m.Attachment
                    })
                    .Take(1)
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid[]> GetUserIdsFromChatNoTrackingAsync(string messageChatId)
    {
        return await _dbContext.Set<Chat>()
            .AsNoTracking()
            .Where(c => c.Id.ToString() == messageChatId)
            .SelectMany(c => c.Users)
            .Select(u => u.Id)
            .ToArrayAsync();
    }
}