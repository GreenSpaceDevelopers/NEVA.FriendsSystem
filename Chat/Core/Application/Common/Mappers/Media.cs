using Application.Dtos;
using Domain.Models.Messaging;

namespace Application.Common.Mappers;

public static class Media
{
    public static MediaDto ToDto(this ReactionType reaction, string url)
    {
        return new MediaDto
        {
            Id = reaction.Id,
            Url = url,
            Type = nameof(ReactionType),
        };
    }
    
    public static MediaDto ToDto(this Attachment attachment, string url)
    {
        return new MediaDto
        {
            Id = attachment.Id,
            Url = url,
            Type = attachment.Type.TypeName,
        };
    }
}