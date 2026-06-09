using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Infrastructure.Identity;
using PromptifyWebApi.Infrastructure.Messaging;
using PromptifyWebApi.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IUser, SystemUser>();
builder.AddInfrastructureServices();
builder.AddPromptifyMessaging(x => x.AddConsumer<ProcessPromptConsumer>());

var host = builder.Build();
host.Run();
