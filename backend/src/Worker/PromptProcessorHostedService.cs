using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Domain.Enums;
using PromptifyWebApi.Infrastructure.Data;
using PromptifyWebApi.Infrastructure.Prompts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PromptifyWebApi.Worker;

public class PromptProcessorHostedService : BackgroundService
{
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PromptProcessorHostedService> _logger;
    private readonly TimeProvider _timeProvider;

    public PromptProcessorHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<PromptProcessorHostedService> logger,
        TimeProvider timeProvider)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Prompt processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processed = await TryProcessNextPromptAsync(stoppingToken);
                if (!processed)
                {
                    await Task.Delay(IdleDelay, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing prompt");
                await Task.Delay(IdleDelay, stoppingToken);
            }
        }

        _logger.LogInformation("Prompt processor stopped");
    }

    private async Task<bool> TryProcessNextPromptAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var claimService = scope.ServiceProvider.GetRequiredService<PromptClaimService>();
        var llmClient = scope.ServiceProvider.GetRequiredService<ILlmClient>();

        var prompt = await claimService.TryClaimNextAsync(cancellationToken);
        if (prompt is null)
        {
            return false;
        }

        await PromptStatusNotifier.NotifyAsync(context, prompt, cancellationToken);

        try
        {
            var output = await llmClient.CompleteAsync(prompt.Input, cancellationToken);
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

        await context.SaveChangesAsync(cancellationToken);
        await PromptStatusNotifier.NotifyAsync(context, prompt, cancellationToken);

        return true;
    }
}
