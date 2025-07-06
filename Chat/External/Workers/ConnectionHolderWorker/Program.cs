using Application;
using Application.Abstractions.Services.Communications;
using Application.Abstractions.Services.Communications.Data;
using Application.Abstractions.Services.Net;
using Application.Dtos.Messaging;
using Application.Messaging.Proto.Messages;
using Google.Protobuf;
using GS.CommonLibrary;
using GS.CommonLibrary.Config;
using GS.IdentityServerApi.Extensions;
using Infrastructure;
using Infrastructure.Services.ApplicationInfrastructure.Network;
using Infrastructure.Services.ApplicationInfrastructure.Queues;
using MessagePack;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddQueue<RawMessage>();

MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard
    .WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);

builder.Services.Configure<string>( builder.Configuration.GetSection("WebSocketHost"));
builder.Services.Configure<QueueConfig<RawMessage>>(builder.Configuration.GetSection(QueueConfig<RawMessage>.SectionName));

var section = builder.Configuration.GetSection(QueueConfig<RawMessage>.SectionName);

Console.WriteLine("Host: " + builder.Configuration[QueueConfig<RawMessage>.SectionName]);
Console.WriteLine(section.Value);

builder.Services.AddHostedService<ConnectionHolderWorker.ConnectionHolderWorker>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
var identityClientBaseUrl = builder.Configuration["IdentityClient:BaseUrl"] ?? "localhost";
builder.Services.AddIdentityClient(identityClientBaseUrl);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IConnectionHolder, ConnectionHolder>();
builder.Services.AddSingleton<IWebSocketStore, WebSocketStore>();
builder.Services.AddSingleton<IRawMessagesQueue, RawMessagesQueue>();

var host = builder.Build();

using var scope = host.Services.CreateScope();

var options = scope.ServiceProvider.GetRequiredService<IOptions<QueueConfig<RawMessage>>>();

Console.WriteLine(options.Value.Host);
var request = new ConnectionRequest
{
    OptionalConnectionId = "3f5c4b62-2a41-4b6c-a3ae-416f9b3a6079",
    MessageId = "9a78fd56-4d26-49eb-b1ef-d47b6e177d0d",
    AccessToken = "my-secret-token",
    Hash = "xY7aPq39Lm5Ws8Jv"
};

using var output = new MemoryStream();
request.WriteTo(output);
byte[] protobufBytes = output.ToArray();

string base64 = Convert.ToBase64String(protobufBytes);
Console.WriteLine(base64);
await host.RunAsync();