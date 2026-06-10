using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Infrastructure.Llm;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shouldly;

namespace PromptifyWebApi.Application.UnitTests.Infrastructure.Llm;

public class LlmClientRegistrationTests
{
    [TestCase("Mock", typeof(MockLlmClient))]
    [TestCase("OpenAI", typeof(OpenAiLlmClient))]
    [TestCase("Ollama", typeof(OllamaLlmClient))]
    public void AddLlmClient_ShouldResolveProviderImplementation(string provider, Type expectedType)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Llm:Provider"] = provider,
                ["Llm:OpenAiApiKey"] = "test-key",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLlmClient(configuration);

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var client = serviceProvider.GetRequiredService<ILlmClient>();

        client.GetType().ShouldBe(expectedType);
    }

    [Test]
    public async Task AddLlmClient_WhenOpenAiProviderMissingApiKey_ShouldFailValidationOnStartup()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Llm:Provider"] = "OpenAI",
        });

        builder.Services.AddLlmClient(builder.Configuration);

        using var host = builder.Build();

        var act = async () => await host.StartAsync();

        await act.ShouldThrowAsync<OptionsValidationException>();
    }
}
