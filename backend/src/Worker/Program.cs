using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Infrastructure.Identity;
using PromptifyWebApi.Infrastructure.Messaging;
using PromptifyWebApi.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IUser, SystemUser>();
builder.AddInfrastructureServices();
builder.AddPromptifyMessaging(_ => { });
builder.Services.AddHostedService<PromptProcessorHostedService>();

var host = builder.Build();
host.Run();
