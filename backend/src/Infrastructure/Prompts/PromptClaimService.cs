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

    public Task<Prompt?> ClaimPromptByIdAsync(int promptId, CancellationToken cancellationToken)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return strategy.ExecuteAsync(async () =>
        {
            var now = _timeProvider.GetUtcNow();

            var rowsUpdated = await _context.Prompts
                .Where(p => p.Id == promptId && p.Status == PromptStatus.Pending)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(p => p.Status, PromptStatus.Processing)
                        .SetProperty(p => p.ProcessingStartedAt, now)
                        .SetProperty(p => p.LastModified, now),
                    cancellationToken);

            if (rowsUpdated == 0)
            {
                return null;
            }

            var prompt = await _context.Prompts
                .FirstOrDefaultAsync(p => p.Id == promptId, cancellationToken);

            if (prompt is null)
            {
                return null;
            }

            _logger.LogInformation(
                "Claimed prompt {PromptId} in session {SessionId}",
                prompt.Id,
                prompt.SessionId);

            return prompt;
        });
    }
}
