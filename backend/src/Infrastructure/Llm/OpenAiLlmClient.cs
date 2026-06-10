using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Application.Common.Llm;
using Microsoft.Extensions.Options;

namespace PromptifyWebApi.Infrastructure.Llm;

public class OpenAiLlmClient : ILlmClient
{
    private readonly ChatClient _chatClient;

    public OpenAiLlmClient(IOptions<LlmOptions> options)
    {
        var llmOptions = options.Value;

        if (string.IsNullOrWhiteSpace(llmOptions.OpenAiApiKey))
        {
            throw new InvalidOperationException(
                "Llm:OpenAiApiKey is required when Llm:Provider is OpenAI.");
        }

        if (!string.IsNullOrWhiteSpace(llmOptions.OpenAiBaseUrl))
        {
            var openAiClient = new OpenAIClient(
                new ApiKeyCredential(llmOptions.OpenAiApiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(llmOptions.OpenAiBaseUrl),
                });

            _chatClient = openAiClient.GetChatClient(llmOptions.OpenAiModel);
            return;
        }

        _chatClient = new ChatClient(llmOptions.OpenAiModel, llmOptions.OpenAiApiKey);
    }

    public async Task<string> CompleteAsync(
        IReadOnlyList<LlmMessage> messages,
        CancellationToken cancellationToken)
    {
        var chatMessages = messages.Select(MapMessage).ToList();
        ChatCompletion completion = await _chatClient.CompleteChatAsync(
            chatMessages,
            cancellationToken: cancellationToken);

        return completion.Content[0].Text ?? string.Empty;
    }

    private static ChatMessage MapMessage(LlmMessage message)
    {
        return message.Role switch
        {
            LlmRole.System => new SystemChatMessage(message.Content),
            LlmRole.User => new UserChatMessage(message.Content),
            LlmRole.Assistant => new AssistantChatMessage(message.Content),
            _ => new UserChatMessage(message.Content),
        };
    }
}
