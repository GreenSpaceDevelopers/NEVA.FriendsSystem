using Application;
using Infrastructure;
using WebApi.Endpoints;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using WebApi.Swagger;
using System.Reflection;
using WebApi.Middlewares;
using GS.IdentityServerApi.Extensions;
using Infrastructure.Services.Communications;
using Microsoft.OpenApi.Models;

public static class Program
{
    private const string CorsName = "Neva.Chat";

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplication();

        builder.Services.AddInfrastructure(builder.Configuration);

        var identityClientBaseUrl = builder.Configuration["IdentityClient:BaseUrl"] ?? "";
        builder.Services.AddIdentityClient(identityClientBaseUrl);
        
        builder.Services.AddHttpClient();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSignalR();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "NEVA Chat API",
                Version = "v1.0.0",
                Description = "REST API для чат-приложения NEVA с функциями друзей, постов, комментариев и обмена сообщениями",
            });

            c.SwaggerGeneratorOptions.DescribeAllParametersInCamelCase = true;
            c.OperationFilter<FileUploadOperationFilter>();
            c.AddSecurityDefinition("Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter here your token with bearer format Bearer [token]"
                });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
            c.EnableAnnotations();

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
            c.OperationFilter<Swashbuckle.AspNetCore.Annotations.AnnotationsOperationFilter>();
        });

        builder.Services.AddCors(o =>
            o.AddPolicy(CorsName, builder =>
            {
                builder
                    .WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:9000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("set-cookie");
            }));

        var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ChatsDbContext>();

        try
        {
            await context.Database.MigrateAsync();
            Console.WriteLine("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying database migrations: {ex.Message}");
            throw;
        }
        app.UseMiddleware<ExceptionHandlesMiddleware>();
        app.UseCors(CorsName);
        app.UseStaticFiles();
        app.UseMiddleware<SessionValidationMiddleware>();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chat API V1");
            c.RoutePrefix = "swagger";
        });

        app.UsePathBase("/api");
        
        app.MapChatsEndpoints();
        app.MapBlogEndpoints();
        app.MapMediaEndpoints();
        app.MapFriendsEndpoints();
        app.MapProfileEndpoints();
        app.MapBlackListEndpoints();
        app.MapNotificationSettingsEndpoints();
        app.MapPrivacyEndpoints();
        app.MapUserChatSettingsEndpoints();
        app.MapAdminEndpoints();

        app.MapHub<ChatHub>("/chatHub");

        await app.RunAsync();
    }
}

