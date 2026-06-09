using PromptifyWebApi.Domain.Entities;
using PromptifyWebApi.Domain.Enums;
using PromptifyWebApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PromptifyWebApi.Infrastructure.Prompts;

public class PromptClaimService
{
    private readonly ApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<PromptClaimService> _logger;

    public PromptClaimService(
        ApplicationDbContext context,
        TimeProvider timeProvider,
        ILogger<PromptClaimService> logger)
    {
        _context = context;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Prompt?> TryClaimNextAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var pending = nameof(PromptStatus.Pending);
        var processing = nameof(PromptStatus.Processing);
        var now = _timeProvider.GetUtcNow();

        var promptId = await _context.Database
            .SqlQuery<int?>($"""
                SELECT p."Id" AS "Value"
                FROM "Prompts" AS p
                WHERE p."Status" = {pending}
                  AND NOT EXISTS (
                    SELECT 1
                    FROM "Prompts" AS earlier
                    WHERE earlier."SessionId" = p."SessionId"
                      AND earlier."OrderIndex" < p."OrderIndex"
                      AND earlier."Status" IN ({pending}, {processing})
                  )
                ORDER BY p."Created"
                LIMIT 1
                FOR UPDATE OF p SKIP LOCKED
                """)
            .FirstOrDefaultAsync(cancellationToken);

        if (promptId is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        var prompt = await _context.Prompts
            .FirstOrDefaultAsync(p => p.Id == promptId, cancellationToken);

        if (prompt is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        prompt.Status = PromptStatus.Processing;
        prompt.ProcessingStartedAt = now;
        prompt.LastModified = now;

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Claimed prompt {PromptId} in session {SessionId}", prompt.Id, prompt.SessionId);

        return prompt;
    }
}
