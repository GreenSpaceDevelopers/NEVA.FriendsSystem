using Application.Requests.Commands.Profile;
using Application.Requests.Commands.Privacy;
using Domain.Models.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using System.Text.Json;
using Domain.Models.Messaging;
using GS.IdentityServerApi.Constants;
using WebApi.Common.Helpers;

namespace WebApi.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin").WithTags("Admin");

        group.MapPost("/sync-all-users", SyncAllUsersFromIdentity)
            .WithName("SyncAllUsersFromIdentity");
            
        group.MapPost("/create-privacy-settings", CreatePrivacySettingsForAllUsers)
            .WithName("CreatePrivacySettingsForAllUsers");
            
        group.MapGet("/identity-diagnostics", GetIdentityDiagnostics)
            .WithName("GetIdentityDiagnostics");
            
        group.MapGet("/auth-diagnostics", GetAuthDiagnostics)
            .WithName("GetAuthDiagnostics");

        group.MapPost("/seed-attachment-types", SeedAttachmentTypes)
            .WithName("SeedAttachmentTypes");
    }

    private static async Task<IResult> CreatePrivacySettingsForAllUsers(
        ISender mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreatePrivacySettingsForAllUsersCommand();
            var result = await mediator.SendAsync(command, cancellationToken);
            return result.ToApiResult();
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Ошибка при создании настроек приватности: {ex.Message}");
        }
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

    private static async Task<IResult> GetIdentityDiagnostics(
        HttpClient httpClient,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var diagnosticsResult = new IdentityDiagnosticsResult();
        
        try
        {
            var identityBaseUrl = configuration["IdentityClient:BaseUrl"] ?? "Not configured";
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Not configured";
            var redisConnectionString = configuration.GetConnectionString("RedisHost") ?? "Not configured";
            
            diagnosticsResult.Settings = new IdentitySettings
            {
                IdentityBaseUrl = identityBaseUrl,
                DatabaseConnectionString = connectionString,
                RedisConnectionString = redisConnectionString
            };

            if (identityBaseUrl != "Not configured")
            {
                var tests = new List<ConnectionTest>();
                
                try
                {
                    var baseResponse = await httpClient.GetAsync(identityBaseUrl, cancellationToken);
                    tests.Add(new ConnectionTest
                    {
                        Name = "Base URL",
                        Url = identityBaseUrl,
                        StatusCode = (int)baseResponse.StatusCode,
                        StatusMessage = baseResponse.ReasonPhrase ?? "Unknown",
                        IsSuccessful = baseResponse.IsSuccessStatusCode
                    });
                }
                catch (Exception ex)
                {
                    tests.Add(new ConnectionTest
                    {
                        Name = "Base URL",
                        Url = identityBaseUrl,
                        StatusMessage = $"Exception: {ex.Message}",
                        IsSuccessful = false
                    });
                }

                try
                {
                    var testUrl = $"{identityBaseUrl}/User/GetAllUsers";
                    var response = await httpClient.GetAsync(testUrl, cancellationToken);
                    
                    var test = new ConnectionTest
                    {
                        Name = "GetAllUsers API",
                        Url = testUrl,
                        StatusCode = (int)response.StatusCode,
                        StatusMessage = response.ReasonPhrase ?? "Unknown",
                        IsSuccessful = response.IsSuccessStatusCode
                    };

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync(cancellationToken);
                        test.ResponsePreview = content.Length > 200 ? content.Substring(0, 200) + "..." : content;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        test.ResponsePreview = errorContent.Length > 200 ? errorContent.Substring(0, 200) + "..." : errorContent;
                    }
                    
                    tests.Add(test);
                }
                catch (Exception ex)
                {
                    tests.Add(new ConnectionTest
                    {
                        Name = "GetAllUsers API",
                        Url = $"{identityBaseUrl}/User/GetAllUsers",
                        StatusMessage = $"Exception: {ex.Message}",
                        IsSuccessful = false
                    });
                }

                try
                {
                    var swaggerUrl = $"{identityBaseUrl.Replace("/api", "")}/swagger";
                    var swaggerResponse = await httpClient.GetAsync(swaggerUrl, cancellationToken);
                    tests.Add(new ConnectionTest
                    {
                        Name = "Swagger UI",
                        Url = swaggerUrl,
                        StatusCode = (int)swaggerResponse.StatusCode,
                        StatusMessage = swaggerResponse.ReasonPhrase ?? "Unknown",
                        IsSuccessful = swaggerResponse.IsSuccessStatusCode
                    });
                }
                catch (Exception ex)
                {
                    tests.Add(new ConnectionTest
                    {
                        Name = "Swagger UI",
                        Url = $"{identityBaseUrl.Replace("/api", "")}/swagger",
                        StatusMessage = $"Exception: {ex.Message}",
                        IsSuccessful = false
                    });
                }
                
                diagnosticsResult.ConnectionTests = tests;
                
                var getAllUsersTest = tests.FirstOrDefault(t => t.Name == "GetAllUsers API");
                if (getAllUsersTest != null)
                {
                    diagnosticsResult.IdentityConnection = new ConnectionStatus
                    {
                        IsConnected = getAllUsersTest.IsSuccessful,
                        StatusCode = getAllUsersTest.StatusCode,
                        StatusMessage = getAllUsersTest.StatusMessage,
                        TestUrl = getAllUsersTest.Url,
                        ResponseTime = DateTime.UtcNow,
                        ResponsePreview = getAllUsersTest.ResponsePreview
                    };
                }
                else
                {
                    diagnosticsResult.IdentityConnection = new ConnectionStatus
                    {
                        IsConnected = false,
                        StatusMessage = "No tests performed"
                    };
                }
            }
            else
            {
                diagnosticsResult.IdentityConnection = new ConnectionStatus
                {
                    IsConnected = false,
                    StatusMessage = "Identity BaseUrl not configured"
                };
            }

            diagnosticsResult.IsHealthy = diagnosticsResult.IdentityConnection.IsConnected && 
                                          diagnosticsResult.ConnectionTests.Any(t => t.IsSuccessful);
            diagnosticsResult.CheckedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            diagnosticsResult.IdentityConnection = new ConnectionStatus
            {
                IsConnected = false,
                StatusMessage = $"Exception: {ex.Message}"
            };
            diagnosticsResult.IsHealthy = false;
            diagnosticsResult.CheckedAt = DateTime.UtcNow;
        }
        
        return Results.Ok(diagnosticsResult);
    }

    private static async Task<IResult> SeedAttachmentTypes(
        ChatsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var defaultTypes = new List<(string TypeName, string Extension)>
        {
            ("Image", ".png"),
            ("Video", ".mp4"),
            ("Audio", ".mp3"),
            ("Sticker", ".webp")
        };

        var existing = await dbContext.AttachmentTypes
            .ToListAsync(cancellationToken);

        var toAdd = defaultTypes
            .Where(t => existing.All(e => e.TypeName != t.TypeName))
            .Select(t => new AttachmentType
            {
                Id = Guid.NewGuid(),
                TypeName = t.TypeName,
                Extension = t.Extension
            })
            .ToList();

        if (toAdd.Count == 0)
        {
            return Results.Ok("AttachmentTypes already seeded");
        }

        await dbContext.AttachmentTypes.AddRangeAsync(toAdd, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new { Added = toAdd.Count });
    }

    private static Task<IResult> GetAuthDiagnostics(HttpContext context)
    {
        var authDiagnostics = new AuthDiagnosticsResult();
        
        try
        {
            var authHeader = context.Request.Headers.Authorization.ToString();
            authDiagnostics.HasAuthorizationHeader = !string.IsNullOrEmpty(authHeader);
            authDiagnostics.AuthorizationHeader = string.IsNullOrEmpty(authHeader) ? "Not provided" : authHeader;
            
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader["Bearer ".Length..].Trim();
                authDiagnostics.BearerToken = token;
                authDiagnostics.IsValidBearerFormat = true;
                authDiagnostics.IsValidGuid = Guid.TryParse(token, out var tokenGuid);
                authDiagnostics.TokenAsGuid = tokenGuid.ToString();
            }
            else
            {
                authDiagnostics.IsValidBearerFormat = false;
                authDiagnostics.BearerToken = "Not Bearer format or missing";
            }
            
            authDiagnostics.HasSessionContext = context.Items["SessionContext"] != null;
            if (context.Items["SessionContext"] != null)
            {
                authDiagnostics.SessionContextType = context.Items["SessionContext"].GetType().Name;
            }
            
            authDiagnostics.CurrentUserId = context.GetUserId().ToString();
            authDiagnostics.IsUserAuthenticated = context.GetUserId() != Guid.Empty;
            
            authDiagnostics.CheckedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            authDiagnostics.Error = ex.Message;
        }
        
        return Task.FromResult(Results.Ok(authDiagnostics));
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

public class IdentityDiagnosticsResult
{
    public bool IsHealthy { get; set; }
    public DateTime CheckedAt { get; set; }
    public IdentitySettings Settings { get; set; } = new();
    public ConnectionStatus IdentityConnection { get; set; } = new();
    public List<ConnectionTest> ConnectionTests { get; set; } = new();
}

public class IdentitySettings
{
    public string IdentityBaseUrl { get; set; } = string.Empty;
    public string DatabaseConnectionString { get; set; } = string.Empty;
    public string RedisConnectionString { get; set; } = string.Empty;
}

public class ConnectionStatus
{
    public bool IsConnected { get; set; }
    public int? StatusCode { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public string? TestUrl { get; set; }
    public DateTime? ResponseTime { get; set; }
    public string? ResponsePreview { get; set; }
}

public class ConnectionTest
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public int? StatusCode { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public string? ResponsePreview { get; set; }
}

public class AuthDiagnosticsResult
{
    public bool HasAuthorizationHeader { get; set; }
    public string AuthorizationHeader { get; set; } = string.Empty;
    public bool IsValidBearerFormat { get; set; }
    public string BearerToken { get; set; } = string.Empty;
    public bool IsValidGuid { get; set; }
    public string TokenAsGuid { get; set; } = string.Empty;
    public bool HasSessionContext { get; set; }
    public string? SessionContextType { get; set; }
    public string CurrentUserId { get; set; } = string.Empty;
    public bool IsUserAuthenticated { get; set; }
    public DateTime CheckedAt { get; set; }
    public string? Error { get; set; }
} 