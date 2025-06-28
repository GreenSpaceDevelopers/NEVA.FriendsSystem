using Application.Abstractions.Persistence.Repositories.Media;
using Domain.Models.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Media;

public class AttachmentsRepository(ChatsDbContext dbContext) : BaseRepository<Attachment>(dbContext), IAttachmentsRepository
{
    public async Task<AttachmentType> GetAttachmentTypeAsync(AttachmentTypes attachmentType, CancellationToken cancellationToken = default)
    {
        var type = await dbContext.Set<AttachmentType>()
            .FirstOrDefaultAsync(at => at.TypeName == attachmentType.ToString(), cancellationToken);
        
        if (type == null)
        {
            throw new InvalidOperationException($"Attachment type {attachmentType} not found");
        }
        
        return type;
    }
} 