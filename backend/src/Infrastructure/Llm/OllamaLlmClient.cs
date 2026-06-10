using System.Net.Http.Json;
using System.Text.Json.Serialization;
using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Application.Common.Llm;
using Microsoft.Extensions.Options;

namespace PromptifyWebApi.Infrastructure.Llm;

public class OllamaLlmClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly LlmOptions _options;

    public OllamaLlmClient(HttpClient httpClient, IOptions<LlmOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> CompleteAsync(
        IReadOnlyList<LlmMessage> messages,
        CancellationToken cancellationToken)
    {
        var request = new OllamaChatRequest(
            _options.OllamaModel,
            messages.Select(MapMessage).ToList(),
            Stream: false);

        using var response = await _httpClient.PostAsJsonAsync(
            "api/chat",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
            cancellationToken: cancellationToken);

        if (body?.Message?.Content is not { Length: > 0 } content)
        {
            throw new InvalidOperationException("Ollama returned an empty response.");
        }

        return content;
    }

    private static OllamaChatMessage MapMessage(LlmMessage message)
    {
        var role = message.Role switch
        {
            LlmRole.System => "system",
            LlmRole.Assistant => "assistant",
            _ => "user",
        };

        return new OllamaChatMessage(role, message.Content);
    }

    private sealed record OllamaChatRequest(
        string Model,
        IReadOnlyList<OllamaChatMessage> Messages,
        bool Stream);

    private sealed record OllamaChatMessage(string Role, string Content);

    private sealed class OllamaChatResponse
    {
        [JsonPropertyName("message")]
        public OllamaChatMessage? Message { get; init; }
    }
}
