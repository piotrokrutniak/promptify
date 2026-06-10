using MassTransit;
using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Infrastructure.Consumers;
using PromptifyWebApi.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PromptifyWebApi.Application.FunctionalTests.Infrastructure;

public class WebApiFactory(string connectionString) : WebApplicationFactory<WebApplicationEntryPoint>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("FunctionalTesting");

        builder
            .UseSetting("ConnectionStrings:PromptifyWebApiDb", connectionString)
            .UseSetting("Llm:MockDelayMs", "0")
            .UseSetting("FunctionalTesting:DisableMessaging", "true");

        builder.ConfigureTestServices(services =>
        {
            services
                .RemoveAll<IUser>()
                .AddTransient(provider =>
                {
                    var mock = new Mock<IUser>();
                    mock.SetupGet(x => x.Roles).Returns(TestApp.GetRoles());
                    mock.SetupGet(x => x.Id).Returns(TestApp.GetUserId());
                    return mock.Object;
                });

            services.RemoveAll<IPublishEndpoint>();
            services.AddSingleton(Mock.Of<IPublishEndpoint>());
            services.AddScoped<ProcessPromptConsumer>();
        });
    }
}
