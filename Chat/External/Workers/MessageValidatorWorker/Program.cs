using MessageValidatorWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<MessageValidatorWorker.MessageReceiverWorker>();

var host = builder.Build();
host.Run();