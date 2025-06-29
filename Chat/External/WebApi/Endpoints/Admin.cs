using Application.Requests.Commands.Profile;
using Domain.Models.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using System.Text.Json;
using GS.IdentityServerApi.Constants;

namespace WebApi.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin").WithTags("Admin");

        group.MapPost("/sync-all-users", SyncAllUsersFromIdentity)
            .WithName("SyncAllUsersFromIdentity");
    }

    private static async Task<IResult> SyncAllUsersFromIdentity(
        HttpClient httpClient,
        IConfiguration configuration,
        ISender mediator,
        ChatsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var identityBaseUrl = configuration["IdentityClient:BaseUrl"] ?? "http://localhost:5108/api";
            var getAllUsersUrl = $"{identityBaseUrl}/User/GetAllUsers";

            var response = await httpClient.GetAsync(getAllUsersUrl, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return Results.BadRequest($"Ошибка при запросе к Identity сервису: {response.StatusCode} - {response.ReasonPhrase}");
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var identityResponse = JsonSerializer.Deserialize<IdentityApiResponse>(jsonContent, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            if (identityResponse?.Data == null || identityResponse.Data.Count == 0)
            {
                return Results.BadRequest("Не удалось получить пользователей из Identity сервиса или список пуст");
            }

            var results = new List<SyncResult>();
            
            foreach (var user in identityResponse.Data)
            {
                var existingUser = await dbContext.Set<ChatUser>()
                    .Include(u => u.AspNetUser)
                    .FirstOrDefaultAsync(u => u.AspNetUser.Id == user.UserId, cancellationToken);

                if (existingUser != null)
                {
                    results.Add(new SyncResult 
                    { 
                        UserId = user.UserId,
                        UserName = user.UserLogin,
                        Status = "Skipped",
                        Message = "Пользователь уже существует"
                    });
                    continue;
                }

                try
                {
                    var roleId = await GetChatRoleIdAsync(user.UserRoles, dbContext);
                    
                    var aspNetUser = new AspNetUser
                    {
                        Id = user.UserId,
                        ChatUserId = user.UserId,
                        UserName = user.UserLogin,
                        Email = user.UserEmail ?? "",
                        RoleId = roleId
                    };

                    var createProfileCommand = new CreateProfileCommand(aspNetUser);
                    await mediator.SendAsync(createProfileCommand, cancellationToken);

                    results.Add(new SyncResult 
                    { 
                        UserId = user.UserId,
                        UserName = user.UserLogin,
                        Status = "Created",
                        Message = $"Пользователь создан с ролью: {string.Join(", ", user.UserRoles)}"
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new SyncResult 
                    { 
                        UserId = user.UserId,
                        UserName = user.UserLogin,
                        Status = "Error",
                        Message = ex.Message
                    });
                }
            }

            var summary = new
            {
                Total = identityResponse.Data.Count,
                Created = results.Count(r => r.Status == "Created"),
                Skipped = results.Count(r => r.Status == "Skipped"),
                Errors = results.Count(r => r.Status == "Error"),
                Results = results
            };

            return Results.Ok(summary);
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Ошибка при синхронизации пользователей: {ex.Message}");
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
            dbContext.ChatRoles.AddRange(rolesToCreate);
            await dbContext.SaveChangesAsync();
        }
    }
}

public class IdentityApiResponse
{
    public bool Success { get; set; }
    public List<UserModelSync> Data { get; set; } = [];
    public string? ErrorMessage { get; set; }
}

public class UserModelSync
{
    public Guid UserId { get; set; }
    public string UserLogin { get; set; } = null!;
    public string? UserEmail { get; set; }
    public DateTime? UserRegDate { get; set; }
    public bool? UserEmailConfirmed { get; set; }
    public List<string> UserRoles { get; set; } = [];
}

public class SyncResult
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Message { get; set; } = null!;
} 