using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Persistence.Repositories.Users;
using Infrastructure.Configs;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories.Messaging;
using Infrastructure.Persistence.Repositories.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Infrastructure;

public static class InfrastructureInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IChatsRepository, ChatsRepository>();
        services.AddScoped<IChatUsersRepository, ChatChatUsersRepository>();
        
        services.AddDbContext<ChatsDbContext>(opt =>
            opt.UseSqlite("Data Source=chats.db", o =>
            {
                o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            }));
    }
    
    
    private static void AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisOptions = new RedisConfig();
        configuration.GetSection(RedisConfig.SectionName).Bind(redisOptions);

        var connectionMultiplexer = ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
        services.AddSingleton<IConnectionMultiplexer, ConnectionMultiplexer>(provider => connectionMultiplexer);
    }
}