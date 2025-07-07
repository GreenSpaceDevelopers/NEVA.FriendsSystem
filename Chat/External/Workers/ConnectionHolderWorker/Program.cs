using Application;
using Application.Abstractions.Services.Auth;
using Application.Abstractions.Services.Communications;
using Application.Abstractions.Services.Communications.Data;
using Application.Abstractions.Services.Net;
using Application.Dtos.Messaging;
using Application.Services.Auth;
using GS.CommonLibrary;
using GS.CommonLibrary.Config;
using GS.IdentityServerApi.Extensions;
using Infrastructure;
using Infrastructure.Services.ApplicationInfrastructure.Network;
using Infrastructure.Services.ApplicationInfrastructure.Queues;
using MessagePack;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ConnectionHolderWorker.ConnectionHolderWorker>();

MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard
    .WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);


builder.Services.AddHttpListener(builder.Configuration);
builder.Services.Configure<QueueConfig<RawMessage>>(builder.Configuration.GetSection(QueueConfig<RawMessage>.SectionName));
builder.Services.AddQueue<RawMessage>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
var identityClientBaseUrl = builder.Configuration["IdentityClient:BaseUrl"] ?? "localhost";
builder.Services.AddIdentityClient(identityClientBaseUrl);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IConnectionHolder, ConnectionHolder>();
builder.Services.AddSingleton<IWebSocketStore, WebSocketStore>();
builder.Services.AddSingleton<IRawMessagesQueue, RawMessagesQueue>();
builder.Services.AddSingleton<ITokenValidator, TokenValidator>();

var host = builder.Build();

await host.RunAsync();