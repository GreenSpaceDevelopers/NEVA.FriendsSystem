using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Domain.Models.Messaging;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Messaging;

public class ChatsRepository(ChatsDbContext dbContext) : BaseRepository<Chat>(dbContext), IChatsRepository
{
    public Task<PagedList<Chat>> GetUserChatsNoTrackingAsync(
        Guid userId,
        PageSettings pageSettings,
        string? searchQuery = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<Chat>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.ChatPicture)
            .Include(c => c.Users)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .ThenInclude(m => m.Sender)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .ThenInclude(m => m.Attachment)
            .Where(c => c.Users.Any(u => u.Id == userId));

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(c => 
                c.Name.Contains(searchQuery) || 
                c.Users.Any(u => u.Username.Contains(searchQuery))
            );
        }

        return query
            .OrderByDescending(c => c.LastMessageDate)
            .ToPagedList(pageSettings, cancellationToken);
    }

    public async Task<Guid[]> GetUserIdsFromChatNoTrackingAsync(string messageChatId)
    {
        return await dbContext.Set<Chat>()
            .AsNoTracking()
            .Where(c => c.Id.ToString() == messageChatId)
            .SelectMany(c => c.Users)
            .Select(u => u.Id)
            .ToArrayAsync();
    }

    public Task<List<Message>> GetMessagesByChatIdNoTrackingAsync(Guid chatId, int take, int skip, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Message>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(m => m.Sender)
            .ThenInclude(s => s.Avatar)
            .Include(m => m.Attachment)
            .Include(m => m.Replies)
            .Include(m => m.Reactions)
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<Chat?> GetByIdWithUsersAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Chat>()
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);
    }

    public async Task<Chat?> GetChatPreviewAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Chat>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.ChatPicture)
            .Include(c => c.Users)
            .ThenInclude(u => u.Avatar)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);
    }

    public async Task<PagedList<ChatWithUnreadCount>> GetUserChatsWithUnreadCountAsync(
        Guid userId,
        PageSettings pageSettings,
        string? searchQuery = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<Chat>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.ChatPicture)
            .Include(c => c.Users)
            .ThenInclude(u => u.Avatar)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .ThenInclude(m => m.Sender)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .ThenInclude(m => m.Attachment)
            .Where(c => c.Users.Any(u => u.Id == userId));

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(c => 
                c.Name.Contains(searchQuery) || 
                c.Users.Any(u => u.Username.Contains(searchQuery)));
        }

        var orderedQuery = query.OrderByDescending(c => c.LastMessageDate ?? DateTime.MinValue);
        var totalCount = await orderedQuery.CountAsync(cancellationToken);

        var chats = await orderedQuery
            .Skip(pageSettings.Skip)
            .Take(pageSettings.Take)
            .ToListAsync(cancellationToken);

        // Получаем количество непрочитанных сообщений для каждого чата
        var chatsWithUnreadCount = new List<ChatWithUnreadCount>();
        foreach (var chat in chats)
        {
            var unreadCount = await GetUnreadMessagesCountAsync(userId, chat.Id, cancellationToken);
            chatsWithUnreadCount.Add(new ChatWithUnreadCount(chat, unreadCount));
        }

        return new PagedList<ChatWithUnreadCount>
        {
            Data = chatsWithUnreadCount,
            TotalCount = totalCount
        };
    }

    public async Task<int> GetUnreadMessagesCountAsync(Guid userId, Guid chatId, CancellationToken cancellationToken = default)
    {
        // Получаем настройки пользователя для данного чата
        var userChatSettings = await dbContext.Set<UserChatSettings>()
            .FirstOrDefaultAsync(ucs => ucs.UserId == userId && ucs.ChatId == chatId, cancellationToken);

        // Если настроек нет или LastReadMessageId не установлен, считаем все сообщения непрочитанными
        if (userChatSettings?.LastReadMessageId == null)
        {
            return await dbContext.Set<Message>()
                .Where(m => m.ChatId == chatId && m.SenderId != userId) // Исключаем собственные сообщения
                .CountAsync(cancellationToken);
        }

        // Получаем дату последнего прочитанного сообщения
        var lastReadMessage = await dbContext.Set<Message>()
            .Where(m => m.Id == userChatSettings.LastReadMessageId.Value)
            .Select(m => m.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastReadMessage == default)
        {
            // Если последнее прочитанное сообщение не найдено, считаем все сообщения непрочитанными
            return await dbContext.Set<Message>()
                .Where(m => m.ChatId == chatId && m.SenderId != userId)
                .CountAsync(cancellationToken);
        }

        // Считаем сообщения после последнего прочитанного
        return await dbContext.Set<Message>()
            .Where(m => m.ChatId == chatId && 
                       m.SenderId != userId && 
                       m.CreatedAt > lastReadMessage)
            .CountAsync(cancellationToken);
    }

    public Task<Message?> GetLastMessageInChatAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Message>()
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}