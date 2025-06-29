using Domain.Abstractions;

namespace Domain.Models.Users;

public class ChatRole : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public enum ChatRoles
{
    User = 0,
    Admin = 1,
    Partner = 2
}