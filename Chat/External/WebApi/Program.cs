using Application;
using Infrastructure;
using WebApi.Endpoints;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure();
        
        var app = builder.Build();

        app.UsePathBase("/api");

        app.MapChatsEndpoints();
        app.MapBlogEndpoints();
        app.MapMediaEndpoints();
        app.MapFriendsEndpoints();

        await app.RunAsync();
    }
}

