using Application.Requests.Commands.Profile;
using Domain.Models.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using GS.IdentityServerApi.Constants;

namespace WebApi.Endpoints;

public static class InternalEndpoints
{
    public static void MapInternalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/internal").WithTags("Internal");

        group.MapPost("/create-user", CreateUserFromIdentity)
            .WithName("CreateUserFromIdentityInternal");
    }

    private static async Task<IResult> CreateUserFromIdentity(
        CreateUserRequest request,
        ISender mediator,
        ChatsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var existingUser = await dbContext.Set<ChatUser>()
                .Include(u => u.AspNetUser)
                .FirstOrDefaultAsync(u => u.AspNetUser.Id == request.UserId, cancellationToken);

            if (existingUser != null)
            {
                return Results.Conflict(new { UserId = request.UserId, Message = "Пользователь уже существует" });
            }

            var roleId = await GetChatRoleIdAsync(request.UserRoles, dbContext);
            
            var aspNetUser = new AspNetUser
            {
                Id = request.UserId,
                ChatUserId = request.UserId,
                UserName = request.UserLogin,
                Email = request.UserEmail ?? "",
                RoleId = roleId
            };

            var createProfileCommand = new CreateProfileCommand(aspNetUser);
            await mediator.SendAsync(createProfileCommand, cancellationToken);

            return Results.Ok(new 
            { 
                UserId = request.UserId,
                UserName = request.UserLogin,
                Status = "Created",
                Message = $"Пользователь создан с ролью: {string.Join(", ", request.UserRoles)}"
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Ошибка при создании пользователя: {ex.Message}");
        }
    }

    private static async Task<Guid> GetChatRoleIdAsync(List<string> userRoles, ChatsDbContext dbContext)
    {
        if (userRoles.Contains(UserRoles.Admin.Name))
        {
            var adminRole = await dbContext.ChatRoles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null) return adminRole.Id;
        }
        
        if (userRoles.Contains(UserRoles.Partner.Name))
        {
            var partnerRole = await dbContext.ChatRoles.FirstOrDefaultAsync(r => r.Name == "Partner");
            if (partnerRole != null) return partnerRole.Id;
        }
        
        if (userRoles.Contains(UserRoles.Player.Name))
        {
            var playerRole = await dbContext.ChatRoles.FirstOrDefaultAsync(r => r.Name == "Player");
            if (playerRole != null) return playerRole.Id;
        }
        
        var defaultRole = await dbContext.ChatRoles.FirstOrDefaultAsync(r => r.Name == "User");
        if (defaultRole != null) return defaultRole.Id;
        
        await EnsureChatRolesExist(dbContext);
        var userRole = await dbContext.ChatRoles.FirstOrDefaultAsync(r => r.Name == "User");
        return userRole?.Id ?? Guid.NewGuid();
    }
    
    private static async Task EnsureChatRolesExist(ChatsDbContext dbContext)
    {
        var existingRoles = await dbContext.ChatRoles.ToListAsync();
        var rolesToCreate = new List<ChatRole>();
        
        if (existingRoles.All(r => r.Name != "Admin"))
            rolesToCreate.Add(new ChatRole { Id = Guid.NewGuid(), Name = "Admin" });
            
        if (existingRoles.All(r => r.Name != "Partner"))
            rolesToCreate.Add(new ChatRole { Id = Guid.NewGuid(), Name = "Partner" });
            
        if (existingRoles.All(r => r.Name != "Player"))
            rolesToCreate.Add(new ChatRole { Id = Guid.NewGuid(), Name = "Player" });
            
        if (existingRoles.All(r => r.Name != "User"))
            rolesToCreate.Add(new ChatRole { Id = Guid.NewGuid(), Name = "User" });
        
        if (rolesToCreate.Count != 0)
        {
            await dbContext.ChatRoles.AddRangeAsync(rolesToCreate);
            await dbContext.SaveChangesAsync();
        }
    }
}

public class CreateUserRequest
{
    public Guid UserId { get; set; }
    public string UserLogin { get; set; } = null!;
    public string? UserEmail { get; set; }
    public List<string> UserRoles { get; set; } = [];
} 