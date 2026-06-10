using PromptifyWebApi.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace PromptifyWebApi.Infrastructure.Llm;

public static class LlmServiceCollectionExtensions
{
    public static IServiceCollection AddLlmClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<LlmOptions>()
            .Bind(configuration.GetSection(LlmOptions.SectionName))
            .Validate(
                options =>
                    !string.Equals(options.Provider, "OpenAI", StringComparison.OrdinalIgnoreCase)
                    || !string.IsNullOrWhiteSpace(options.OpenAiApiKey),
                "Llm:OpenAiApiKey is required when Llm:Provider is OpenAI.")
            .ValidateOnStart();

        services.AddHttpClient(OllamaLlmClient.HttpClientName, (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<LlmOptions>>().Value;
            client.BaseAddress = new Uri(options.OllamaBaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddSingleton<MockLlmClient>();
        services.AddSingleton<OpenAiLlmClient>();
        services.AddSingleton<OllamaLlmClient>();
        services.AddSingleton<ILlmClient>(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<IOptions<LlmOptions>>().Value.Provider;

            return provider.Trim() switch
            {
                "OpenAI" => serviceProvider.GetRequiredService<OpenAiLlmClient>(),
                "Ollama" => serviceProvider.GetRequiredService<OllamaLlmClient>(),
                _ => serviceProvider.GetRequiredService<MockLlmClient>(),
            };
        });

        services.AddScoped<ConversationHistoryBuilder>();

        return services;
    }
}
