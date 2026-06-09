using PromptifyWebApi.Application.Common.Exceptions;
using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Domain.Entities;
using PromptifyWebApi.Domain.Enums;

namespace PromptifyWebApi.Application.Sessions;

internal static class SessionAccess
{
    public static async Task<Session> GetOwnedSessionAsync(
        IApplicationDbContext context,
        IUser user,
        int sessionId,
        CancellationToken cancellationToken)
    {
        var session = await context.Sessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session is null)
        {
            throw new EntityNotFoundException(nameof(Session), sessionId);
        }

        if (session.UserId != user.Id)
        {
            throw new ForbiddenAccessException();
        }

        return session;
    }

    public static async Task EnsureSessionIdleAsync(
        IApplicationDbContext context,
        int sessionId,
        CancellationToken cancellationToken)
    {
        var hasActive = await context.Prompts.AnyAsync(
            p => p.SessionId == sessionId &&
                 (p.Status == PromptStatus.Pending || p.Status == PromptStatus.Processing),
            cancellationToken);

        if (hasActive)
        {
            throw new ConflictException("Session has a prompt that is pending or processing.");
        }
    }
}
