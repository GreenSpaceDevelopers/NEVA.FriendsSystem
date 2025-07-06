using Application;
using Infrastructure;
using MessageReceiverWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<MessageReceiverWorker.MessageReceiverWorker>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();
await host.RunAsync();