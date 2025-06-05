using MessageHandlerWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<MessageHandlerWorker.MessageHandlerWorker>();

var host = builder.Build();
host.Run();