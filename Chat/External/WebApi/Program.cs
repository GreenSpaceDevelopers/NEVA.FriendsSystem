using Application;
using Infrastructure;
using WebApi.Endpoints;
using External.WebApi.Endpoints;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using WebApi.Swagger;

public static class Program
{
    private const string CorsName = "Neva.Chat";
    
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add services to the container
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);
        
        // Add Swagger services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Chat API",
                Version = "v1",
                Description = "API for chat application with friends, posts, comments and messaging features"
            });
            
            // Configure Swagger to handle file uploads properly
            c.SwaggerGeneratorOptions.DescribeAllParametersInCamelCase = true;
            c.OperationFilter<FileUploadOperationFilter>();
        });
        
        builder.Services.AddCors(o =>
            o.AddPolicy(CorsName, builder =>
            {
                builder
                    .WithOrigins("http://localhost:5173", "http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("set-cookie");
            }));
        
        var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ChatsDbContext>();
        
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
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
        app.UseCors(CorsName);
        
        // Configure the HTTP request pipeline
        
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

        await app.RunAsync();
    }
}

