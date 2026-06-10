using PromptifyWebApi.Application.Common.Llm;
using PromptifyWebApi.Domain.Enums;
using PromptifyWebApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PromptifyWebApi.Infrastructure.Llm;

public class ConversationHistoryBuilder(ApplicationDbContext context)
{
    public async Task<IReadOnlyList<LlmMessage>> BuildAsync(
        int sessionId,
        int currentOrderIndex,
        string currentInput,
        CancellationToken cancellationToken)
    {
        var priorPrompts = await context.Prompts
            .AsNoTracking()
            .Where(prompt =>
                prompt.SessionId == sessionId &&
                prompt.OrderIndex < currentOrderIndex &&
                prompt.Status == PromptStatus.Completed &&
                prompt.Output != null)
            .OrderBy(prompt => prompt.OrderIndex)
            .Select(prompt => new { prompt.Input, prompt.Output })
            .ToListAsync(cancellationToken);

        var messages = new List<LlmMessage>(priorPrompts.Count * 2 + 1);

        foreach (var priorPrompt in priorPrompts)
        {
            messages.Add(new LlmMessage(LlmRole.User, priorPrompt.Input));
            messages.Add(new LlmMessage(LlmRole.Assistant, priorPrompt.Output!));
        }

        messages.Add(new LlmMessage(LlmRole.User, currentInput));
        return messages;
    }
}
