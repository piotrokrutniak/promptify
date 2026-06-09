using MassTransit;
using PromptifyWebApi.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PromptifyWebApi.Infrastructure.Messaging;

public static class MassTransitExtensions
{
    public static void AddPromptifyMessaging(
        this IHostApplicationBuilder builder,
        Action<IBusRegistrationConfigurator> configureConsumers)
    {
        builder.Services.AddMassTransit(x =>
        {
            configureConsumers(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = builder.Configuration.GetConnectionString(Services.Messaging);
                Guard.Against.NullOrWhiteSpace(connectionString, message: $"Connection string '{Services.Messaging}' not found.");

                cfg.Host(new Uri(connectionString));
                cfg.UseMessageRetry(r => r.Intervals(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(15)));
                cfg.ConfigureEndpoints(context);
            });
        });
    }
}
