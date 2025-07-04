using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Profile;

[SwaggerSchema(Description = "Права взаимодействия с пользователем")] 
public record UserInteractionPermissionsDto(
    [property: SwaggerSchema(Description = "Можно ли написать пользователю (личные сообщения)")] bool CanWriteMessages,
    [property: SwaggerSchema(Description = "Можно ли оставлять комментарии")] bool CanLeaveComments,
    [property: SwaggerSchema(Description = "Можно ли просматривать список друзей")] bool CanViewFriendLists
 ); 