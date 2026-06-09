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

    public async Task<Prompt?> ClaimPromptByIdAsync(int promptId, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var pending = nameof(PromptStatus.Pending);
        var claimedId = await _context.Database
            .SqlQuery<int?>($"""
                SELECT p."Id" AS "Value"
                FROM "Prompts" AS p
                WHERE p."Id" = {promptId}
                  AND p."Status" = {pending}
                FOR UPDATE OF p SKIP LOCKED
                """)
            .FirstOrDefaultAsync(cancellationToken);

        if (claimedId is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        var prompt = await _context.Prompts
            .FirstOrDefaultAsync(p => p.Id == claimedId, cancellationToken);

        if (prompt is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        var now = _timeProvider.GetUtcNow();
        prompt.Status = PromptStatus.Processing;
        prompt.ProcessingStartedAt = now;
        prompt.LastModified = now;

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Claimed prompt {PromptId} in session {SessionId}", prompt.Id, prompt.SessionId);

        return prompt;
    }
}
