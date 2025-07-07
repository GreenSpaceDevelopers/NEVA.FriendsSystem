using Application;
using Application.Abstractions.Services.Auth;
using Application.Abstractions.Services.Communications;
using Application.Abstractions.Services.Communications.Data;
using Application.Abstractions.Services.Net;
using Application.Dtos.Messaging;
using Application.Services.Auth;
using Application.Services.Communication;
using Application.Services.Communication.Data;
using GS.CommonLibrary;
using GS.CommonLibrary.Config;
using GS.CommonLibrary.Services;
using GS.IdentityServerApi.Extensions;
using Infrastructure;
using Infrastructure.Services.ApplicationInfrastructure.Network;
using Infrastructure.Services.ApplicationInfrastructure.Queues;
using MessagePack;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<MessageReceiverWorker.MessageReceiverWorker>();

MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard
    .WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);



builder.Services.AddSingleton<ICacheService, CacheService>(opt =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis");
    var connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString ?? "default redis connection string");
    
    var db = connectionMultiplexer.GetDatabase();
    return new CacheService(db, "users-");
});

builder.Services.Configure<QueueConfig<MessageToRoute>>(builder.Configuration.GetSection(QueueConfig<MessageToRoute>.SectionName));
builder.Services.Configure<QueueConfig<MessageToProcess>>(builder.Configuration.GetSection(QueueConfig<MessageToProcess>.SectionName));
builder.Services.Configure<QueueConfig<RawMessage>>(builder.Configuration.GetSection(QueueConfig<RawMessage>.SectionName));
builder.Services.AddQueue<MessageToRoute>();
builder.Services.AddQueue<RawMessage>();
builder.Services.AddQueue<MessageToProcess>();
builder.Services.AddSingleton<IUserConnectionsCache, UserConnectionsCache>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
var identityClientBaseUrl = builder.Configuration["IdentityClient:BaseUrl"] ?? "localhost";
builder.Services.AddIdentityClient(identityClientBaseUrl);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IWebSocketStore, WebSocketStore>();
builder.Services.AddSingleton<IRawMessagesQueue, RawMessagesQueue>();
builder.Services.AddSingleton<ITokenValidator, TokenValidator>();
builder.Services.AddSingleton<IMessagesToProcessQueue, MessagesToProcessQueue>();
builder.Services.AddSingleton<IMessagesReceiver, MessagesReceiver>();
builder.Services.AddSingleton<ISigningService, SingingService>();
builder.Services.AddSingleton<IMessagesToRouteQueue, MessagesToRouteQueue>();

var host = builder.Build();
await host.RunAsync();