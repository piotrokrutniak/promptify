using MassTransit;
using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Application.Common.Mappings;
using PromptifyWebApi.Domain.Enums;
using PromptifyWebApi.Infrastructure.Data;
using PromptifyWebApi.Infrastructure.Llm;
using PromptifyWebApi.Infrastructure.Prompts;
using PromptifyWebApi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PromptifyWebApi.Infrastructure.Consumers;

public class ProcessPromptConsumer : IConsumer<ProcessPromptCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly PromptClaimService _claimService;
    private readonly ConversationHistoryBuilder _historyBuilder;
    private readonly ILlmClient _llmClient;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ProcessPromptConsumer> _logger;

    public ProcessPromptConsumer(
        ApplicationDbContext context,
        PromptClaimService claimService,
        ConversationHistoryBuilder historyBuilder,
        ILlmClient llmClient,
        IPublishEndpoint publishEndpoint,
        TimeProvider timeProvider,
        ILogger<ProcessPromptConsumer> logger)
    {
        _context = context;
        _claimService = claimService;
        _historyBuilder = historyBuilder;
        _llmClient = llmClient;
        _publishEndpoint = publishEndpoint;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessPromptCommand> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        var existing = await _context.Prompts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == message.PromptId, cancellationToken);

        if (existing is null)
        {
            _logger.LogWarning("Prompt {PromptId} not found", message.PromptId);
            return;
        }

        if (existing.Status == PromptStatus.Cancelled)
        {
            return;
        }

        if (existing.Status != PromptStatus.Pending)
        {
            return;
        }

        var prompt = await _claimService.ClaimPromptByIdAsync(message.PromptId, cancellationToken);
        if (prompt is null)
        {
            return;
        }

        await PublishStatusAsync(prompt, cancellationToken);

        if (await IsCancelledAsync(prompt.Id, cancellationToken))
        {
            return;
        }

        try
        {
            var messages = await _historyBuilder.BuildAsync(
                prompt.SessionId,
                prompt.OrderIndex,
                prompt.Input,
                cancellationToken);

            var output = await _llmClient.CompleteAsync(messages, cancellationToken);

            if (await IsCancelledAsync(prompt.Id, cancellationToken))
            {
                return;
            }

            var now = _timeProvider.GetUtcNow();
            prompt.Status = PromptStatus.Completed;
            prompt.Output = output;
            prompt.CompletedAt = now;
            prompt.LastModified = now;
            prompt.ErrorMessage = null;
        }
        catch (Exception ex)
        {
            var now = _timeProvider.GetUtcNow();
            prompt.Status = PromptStatus.Failed;
            prompt.ErrorMessage = ex.Message;
            prompt.CompletedAt = now;
            prompt.LastModified = now;

            _logger.LogWarning(ex, "Prompt {PromptId} failed", prompt.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await PublishStatusAsync(prompt, cancellationToken);
    }

    private async Task<bool> IsCancelledAsync(int promptId, CancellationToken cancellationToken)
    {
        var status = await _context.Prompts
            .AsNoTracking()
            .Where(p => p.Id == promptId)
            .Select(p => p.Status)
            .FirstOrDefaultAsync(cancellationToken);

        return status == PromptStatus.Cancelled;
    }

    private Task PublishStatusAsync(Domain.Entities.Prompt prompt, CancellationToken cancellationToken)
    {
        return _publishEndpoint.Publish(prompt.ToStatusChanged(), cancellationToken);
    }
}
