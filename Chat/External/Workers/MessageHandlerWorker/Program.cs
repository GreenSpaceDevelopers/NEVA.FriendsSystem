using Application;
using Infrastructure;
using MessageHandlerWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<MessageHandlerWorker.MessageHandlerWorker>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();
host.Run();