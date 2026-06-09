using PromptifyWebApi.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddInfrastructureServices();
builder.Services.AddHostedService<PromptProcessorHostedService>();

var host = builder.Build();
host.Run();
