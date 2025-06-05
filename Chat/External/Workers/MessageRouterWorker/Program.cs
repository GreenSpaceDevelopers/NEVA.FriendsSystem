using MessageRouterWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<MessageRouterWorker.MessageSenderWorker>();

var host = builder.Build();
host.Run();