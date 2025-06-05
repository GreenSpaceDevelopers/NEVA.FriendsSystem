var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ConnectionHolderWorker.ConnectionHolderWorker>();

var host = builder.Build();
host.Run();