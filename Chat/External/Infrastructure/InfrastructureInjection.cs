using System.Net;
using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.Communications;
using Application.Abstractions.Services.Notifications;
using Application.Abstractions.Services.Reporting;
using Infrastructure.Configs;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories.Blog;
using Infrastructure.Persistence.Repositories.Media;
using Infrastructure.Persistence.Repositories.Messaging;
using Infrastructure.Persistence.Repositories.Users;
using Infrastructure.Services.ApplicationInfrastructure.Data;
using Infrastructure.Services.Communications;
using Infrastructure.Services.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Telegram.Bot;

namespace Infrastructure;

public static class InfrastructureInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MinioConfig>(configuration.GetSection(MinioConfig.SectionName));
        services.Configure<TelegramConfig>(configuration.GetSection(TelegramConfig.SectionName));

        services.AddScoped<IChatsRepository, ChatsRepository>();
        services.AddScoped<IChatUsersRepository, ChatChatUsersRepository>();
        services.AddScoped<IBlogRepository, BlogRepository>();
        services.AddScoped<IPrivacyRepository, PrivacyRepository>();
        services.AddScoped<INotificationSettingsRepository, NotificationSettingsRepository>();
        services.AddScoped<IUserChatSettingsRepository, UserChatSettingsRepository>();
        services.AddScoped<IAttachmentsRepository, AttachmentsRepository>();
        services.AddScoped<IReactionsTypesRepository, ReactionsTypesRepository>();
        services.AddScoped<IMessagesRepository, MessagesRepository>();
        services.AddScoped<IBackendNotificationService, BackendNotificationService>();

        services.AddScoped<IFilesStorage, FilesStorage>();
        services.AddScoped<IFilesValidator, FilesValidator>();
        services.AddScoped<IFilesSigningService, FilesSigningService>();
        services.AddScoped<IChatNotificationService, SignalRChatNotificationService>();

        services.AddSingleton<ITelegramBotClient>(provider =>
        {
            var telegramConfig = configuration.GetSection(TelegramConfig.SectionName).Get<TelegramConfig>();
            return new TelegramBotClient(telegramConfig?.BotToken ?? string.Empty);
        });
        services.AddScoped<IBugReporter, TelegramBugReporter>();

        services.AddHostedService<MinioInitializer>();

        services.AddDbContext<ChatsDbContext>(opt =>
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), o =>
            {
                o.CommandTimeout(30);
                o.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            }));
    }

    public static IServiceCollection AddHttpListener(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("WebSocketHost").Value;

        var httpListener = new HttpListener();
        httpListener.Prefixes.Add(connectionString ?? throw new Exception("WebSocketHost is not configured"));
        
        return services.AddSingleton<HttpListener>(provider => httpListener);
    }


    private static void AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisOptions = new RedisConfig();
        configuration.GetSection(RedisConfig.SectionName).Bind(redisOptions);

        var connectionMultiplexer = ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
        services.AddSingleton<IConnectionMultiplexer, ConnectionMultiplexer>(provider => connectionMultiplexer);
    }
}