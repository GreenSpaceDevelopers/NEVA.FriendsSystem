using Application.Dtos;
using Domain.Models.Messaging;

namespace Application.Common.Mappers;

public static class Media
{
    public static MediaDto ToDto(this ReactionType reaction, string url)
    {
        return new MediaDto(
            reaction.Id,
            nameof(ReactionType),
            url,
            string.Empty); // what the hell is that?
    }

    public static MediaDto ToDto(this Attachment attachment, string url)
    {
        return new MediaDto(
            attachment.Id,
            attachment.Type.TypeName,
            url,
            string.Empty); // what the hell is that?
    }
}