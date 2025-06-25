using Application;
using Infrastructure;
using WebApi.Endpoints;
using External.WebApi.Endpoints;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);
        
        var app = builder.Build();

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

