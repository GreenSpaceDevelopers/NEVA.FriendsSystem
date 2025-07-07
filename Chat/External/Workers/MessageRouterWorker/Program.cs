using Application;
using Infrastructure;
using MessageRouterWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<MessageSenderWorker>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();
await host.RunAsync();