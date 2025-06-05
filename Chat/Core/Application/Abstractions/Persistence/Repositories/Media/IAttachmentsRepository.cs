using Domain.Models.Messaging;
using Attachment = Domain.Models.Messaging.Attachment;

namespace Application.Abstractions.Persistence.Repositories.Media;

public interface IAttachmentsRepository : IBaseRepository<Attachment>
{
    public Task<AttachmentType> GetAttachmentTypeAsync(AttachmentTypes attachmentType, CancellationToken cancellationToken = default);
};